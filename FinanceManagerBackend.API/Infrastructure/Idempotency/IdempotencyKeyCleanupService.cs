using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// Periodically deletes expired idempotency keys.
/// </summary>
public class IdempotencyKeyCleanupService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<IdempotencyKeyCleanupService> logger
) : BackgroundService
{
    /// <summary>
    /// Key lifetime. The client gives up after ~1-2 hours of retries, so a day is a wide margin.
    /// </summary>
    public static readonly TimeSpan KeyTimeToLive = TimeSpan.FromHours(24);

    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        do
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();

                var expiredAt = DateTimeOffset.UtcNow - KeyTimeToLive;

                var deleted = await context.IdempotencyRecords
                    .Where(x => x.CreatedAt < expiredAt)
                    .ExecuteDeleteAsync(stoppingToken);

                logger.LogInformation("Deleted {Count} expired idempotency keys", deleted);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Failed to delete expired idempotency keys");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
