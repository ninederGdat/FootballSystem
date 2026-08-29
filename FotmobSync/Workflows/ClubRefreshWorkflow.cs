using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using FotmobSync.Mappers;
using FotmobSync.Modules;
using FotmobSync.Services;

namespace FotmobSync.Workflows;

public class ClubRefreshWorkflow
{
    private readonly FotmobTeamDataModule _teamDataModule;
    private readonly ITeamSyncService _teamSyncService;
    private readonly IMatchSyncService _matchSyncService;
    private readonly ISquadSyncService _squadSyncService;
    private readonly ITransferSyncService _transferSyncService;
    private readonly ITransferStatusResolver _transferStatusReSolver;
    private readonly ITransferStatusSyncService _transferStatusSyncService;
    private readonly ILogger<ClubRefreshWorkflow> _logger;

    public ClubRefreshWorkflow(
        FotmobTeamDataModule teamDataModule,
        ITeamSyncService teamSyncService,
        IMatchSyncService matchSyncService,
        ISquadSyncService squadSyncService,
        ITransferSyncService transferSyncService,
        ITransferStatusResolver transferStatusResolver,
        ITransferStatusSyncService transferStatusSyncService,
        ILogger<ClubRefreshWorkflow> logger)
    {
        _teamDataModule = teamDataModule;
        _teamSyncService = teamSyncService;
        _matchSyncService = matchSyncService;
        _squadSyncService = squadSyncService;
        _transferSyncService = transferSyncService;
        _transferStatusReSolver = transferStatusResolver;
        _transferStatusSyncService = transferStatusSyncService;

        _logger = logger;
    }

    public async Task ExecuteAsync(int teamId)
    {
        try
        {
            _logger.LogInformation(
                "Starting club refresh workflow for team {TeamId}",
                teamId);

            _logger.LogInformation("Loading team data snapshot for team {TeamId}", teamId);
            var snapshot =
                await _teamDataModule.LoadAsync(teamId);

            _logger.LogInformation("STEP 1: Syncing team data for team {TeamId}", teamId);

            await _teamSyncService.SyncAsync(snapshot);

            _logger.LogInformation("STEP 2: Syncing match data for team {TeamId}", teamId);

            await _matchSyncService.SyncAsync(snapshot);

            _logger.LogInformation("STEP 3: Syncing squad data for team {TeamId}", teamId);

            await _squadSyncService.SyncAsync(snapshot, default);

            _logger.LogInformation("STEP 4: Syncing transfer data for team {TeamId}", teamId);
            await _transferSyncService.SyncAsync(snapshot);
            _logger.LogInformation("STEP 5: Resolving and syncing transfer status for team {TeamId}", teamId);
            var transfersRawList = snapshot.TeamRaw.Transfers.AllTransfers;
            var transferCleanList = transfersRawList.ToCleanList();
            var transferStatuses = _transferStatusReSolver.Resolve(
                                    transferCleanList,
                                    DateTime.UtcNow,
                                    teamId
            );
            await _transferStatusSyncService.SyncAsync(transferStatuses, default);

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