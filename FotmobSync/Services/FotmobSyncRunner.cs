using FotmobSync.Workflows;

namespace FotmobSync.Services;

public class FotmobSyncRunner : IFotmobSyncRunner
{
    private readonly ClubRefreshWorkflow _workflow;
    private readonly ILogger<FotmobSyncRunner> _logger;

    public FotmobSyncRunner(
        ClubRefreshWorkflow workflow,
        ILogger<FotmobSyncRunner> logger)
    {
        _workflow = workflow;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // 8455 - Chelsea FC
        // 9848 - RC Strasbourg Alsace
        var teamsToSync = new[] { 8455, 9848 };

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Syncing teams {TeamIds}", string.Join(",", teamsToSync));

        await _workflow.ExecuteAsync(teamsToSync, cancellationToken);
    }
}