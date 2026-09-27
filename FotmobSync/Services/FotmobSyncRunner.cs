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
        var teamsToSync = new[] { 8455 };

        foreach (var teamId in teamsToSync)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Syncing team with ID {TeamId}",
                teamId);

            await _workflow.ExecuteAsync(teamId);
        }
    }
}