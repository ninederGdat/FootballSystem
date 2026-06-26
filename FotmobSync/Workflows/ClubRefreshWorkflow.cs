using FotmobSync.Modules;
using FotmobSync.Services;

namespace FotmobSync.Workflows;

public class ClubRefreshWorkflow
{
    private readonly FotmobTeamDataModule _teamDataModule;
    private readonly ITeamSyncService _teamSyncService;
    private readonly IMatchSyncService _matchSyncService;
    private readonly ISquadSyncService _squadSyncService;
    private readonly ILogger<ClubRefreshWorkflow> _logger;

    public ClubRefreshWorkflow(
        FotmobTeamDataModule teamDataModule,
        ITeamSyncService teamSyncService,
        IMatchSyncService matchSyncService,
        ISquadSyncService squadSyncService,
        ILogger<ClubRefreshWorkflow> logger)
    {
        _teamDataModule = teamDataModule;
        _teamSyncService = teamSyncService;
        _matchSyncService = matchSyncService;
        _squadSyncService = squadSyncService;
        _logger = logger;
    }

    public async Task ExecuteAsync(int teamId)
    {
        try
        {
            _logger.LogInformation(
                "Starting club refresh workflow for team {TeamId}",
                teamId);

            var snapshot =
                await _teamDataModule.LoadAsync(teamId);

            _logger.LogInformation("STEP 1");

            await _teamSyncService.SyncAsync(snapshot);

            _logger.LogInformation("STEP 2");

            await _matchSyncService.SyncAsync(snapshot);

            _logger.LogInformation("STEP 3");

            await _squadSyncService.SyncAsync(snapshot);

            _logger.LogInformation("STEP 4");

            _logger.LogInformation(
                "Completed club refresh workflow for team {TeamId}",
                teamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error executing workflow for team {TeamId}",
                teamId);
        }
    }
}