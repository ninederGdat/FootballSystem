namespace FotmobSync.Options;

public class QuartzSyncOptions
{
    public const string SectionName = "Quartz";

    /// <summary>Cron for <see cref="Jobs.DailyFotmobSyncJob"/> (default: every 15 minutes).</summary>
    public string CronSchedule { get; set; } = "0 0/15 * * * ?";

    /// <summary>
    /// When true, fire once immediately when the scheduler starts (slow: full squad + Playwright).
    /// When false, first run is at the next cron tick only.
    /// </summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>Delay before the Quartz scheduler starts (lets the host finish boot logging).</summary>
    public int SchedulerStartDelaySeconds { get; set; } = 2;
}
