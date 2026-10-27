using Microsoft.EntityFrameworkCore;
using FinanceManagerBackend.API.Infrastructure.Scheduling;
using Quartz;

namespace FinanceManagerBackend.API.Infrastructure.Idempotency;

/// <summary>
/// Deletes expired idempotency keys every hour.
/// </summary>
[CronJob("0 0 * * * ?")]
[DisallowConcurrentExecution]
public class IdempotencyKeyCleanupJob(
    ServiceDbContext context,
    ILogger<IdempotencyKeyCleanupJob> logger
) : IJob
{
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
