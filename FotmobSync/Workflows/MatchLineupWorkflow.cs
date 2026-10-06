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

    public async Task<bool> ExecuteAsync(int matchId, int teamId)
    {
        var snapshot = await _matchModule.LoadAsync(matchId, teamId);

        var lineup = await _lineupSyncService.SyncAsync(snapshot);
        if (lineup is null) return false;

        var result = await _lineupPlayerSyncService.SyncAsync(snapshot, lineup.Id);

        await _matchEventSyncService.SyncAsync(snapshot, matchId);

        if (!result.IsComplete)
        {
            _logger.LogWarning(
                "Match {MatchId}: lineup chưa đủ ({Starters}/11 starters).",
                matchId, result.Starters);
            return false;
        }

        await _lineupSyncService.MarkCompleteAsync(lineup.Id);
        return true;
    }
}