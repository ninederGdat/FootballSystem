using FotmobSync.Services;
using Quartz;

namespace FotmobSync.Jobs;

[DisallowConcurrentExecution]
public class DailyFotmobSyncJob : IJob
{
    private readonly IFotmobSyncRunner _syncRunner;
    private readonly ILogger<DailyFotmobSyncJob> _logger;

    public DailyFotmobSyncJob(
        IFotmobSyncRunner syncRunner,
        ILogger<DailyFotmobSyncJob> logger)
    {
        _syncRunner = syncRunner;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation(
            "Starting Daily Fotmob Sync Job at {Time}",
            DateTimeOffset.Now);

        try
        {
            await _syncRunner.RunAsync(context.CancellationToken);

            _logger.LogInformation(
                "Completed Daily Fotmob Sync Job at {Time}",
                DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Daily Fotmob Sync Job failed at {Time}",
                DateTimeOffset.Now);

            throw;
        }
    }
}