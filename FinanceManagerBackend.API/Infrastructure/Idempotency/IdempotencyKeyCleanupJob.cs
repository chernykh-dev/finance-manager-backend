using Microsoft.EntityFrameworkCore;
using Quartz;

namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// Deletes expired idempotency keys. Scheduled hourly via Quartz (see <see cref="CronSchedule"/>).
/// </summary>
[DisallowConcurrentExecution]
public class IdempotencyKeyCleanupJob(
    ServiceDbContext context,
    ILogger<IdempotencyKeyCleanupJob> logger
) : IJob
{
    /// <summary>
    /// Job key.
    /// </summary>
    public static readonly JobKey Key = new(nameof(IdempotencyKeyCleanupJob));

    /// <summary>
    /// Every hour at minute 0.
    /// </summary>
    public const string CronSchedule = "0 0 * * * ?";

    /// <summary>
    /// Key lifetime. The client gives up after ~1-2 hours of retries, so a day is a wide margin.
    /// </summary>
    public static readonly TimeSpan KeyTimeToLive = TimeSpan.FromHours(24);

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext jobContext)
    {
        var expiredAt = DateTimeOffset.UtcNow - KeyTimeToLive;

        var deleted = await context.IdempotencyRecords
            .Where(x => x.CreatedAt < expiredAt)
            .ExecuteDeleteAsync(jobContext.CancellationToken);

        logger.LogInformation("Deleted {Count} expired idempotency keys", deleted);
    }
}
