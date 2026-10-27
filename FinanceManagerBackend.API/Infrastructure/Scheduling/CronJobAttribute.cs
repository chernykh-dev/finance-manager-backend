namespace FinanceManagerBackend.API.Infrastructure.Scheduling;

/// <summary>
/// Schedules a Quartz job by cron expression. Jobs marked with it are registered automatically
/// (see ServiceCollectionExtensions.ConfigureJobs).
/// </summary>
/// <param name="cronExpression">Quartz cron expression, e.g. "0 0 * * * ?" (every hour at minute 0).</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CronJobAttribute(string cronExpression) : Attribute
{
    /// <summary>
    /// Quartz cron expression.
    /// </summary>
    public string CronExpression { get; } = cronExpression;
}
