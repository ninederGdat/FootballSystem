using FotmobSync.Modules;
using FotmobSync.Services;

namespace FotmobSync.Workflows;

public class MatchLineupWorkflow
{
    private readonly FotmobMatchDetailModule _matchModule;
    private readonly ILineupSyncService _lineupSyncService;
    private readonly ILineupPlayerSyncService _lineupPlayerSyncService;
    private readonly IMatchEventSyncService _matchEventSyncService;
    private readonly ILogger<MatchLineupWorkflow> _logger;
    public MatchLineupWorkflow(
        FotmobMatchDetailModule matchModule,
        ILineupSyncService lineupSyncService,
        ILineupPlayerSyncService lineupPlayerSyncService,
        IMatchEventSyncService matchEventSyncService,
        ILogger<MatchLineupWorkflow> logger)
    {
        _matchModule = matchModule;
        _lineupSyncService = lineupSyncService;
        _lineupPlayerSyncService = lineupPlayerSyncService;
        _matchEventSyncService = matchEventSyncService;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        int matchId,
        int teamId)
    {
        var snapshot = await _matchModule.LoadAsync(matchId, teamId);


        var lineup = await _lineupSyncService.SyncAsync(snapshot);

        if (lineup is null)
            return;

        _logger.LogInformation("Syncing lineup players for match {MatchId}", matchId);
        await _lineupPlayerSyncService.SyncAsync(
                                    snapshot,
                                    lineup.Id);

       _logger.LogInformation("Syncing match events for match {MatchId}", matchId);
        await _matchEventSyncService.SyncAsync(
                                    snapshot,
                                    matchId);
    }
}