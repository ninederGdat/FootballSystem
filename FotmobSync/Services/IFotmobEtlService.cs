using FotmobSync.Models.Clean;
using FotmobSync.Modules;

namespace FotmobSync.Services;

public interface IFotmobEtlService
{
    Task SyncTeamAsync(int teamId);

    Task SyncMatchesAsync(int teamId);

    Task SyncSquadAsync(int teamId);

    Task SyncPlayerAsync(int playerId, long teamId);
}
public interface ITeamSyncService
{
    Task SyncAsync(TeamDataSnapshot snapshot);
}

public interface IMatchSyncService
{
    Task SyncAsync(TeamDataSnapshot snapshot);
}

public interface ISquadSyncService
{
    Task SyncAsync(TeamDataSnapshot snapshot);
}

public interface IPlayerSyncService
{
    Task SyncAsync(
        int playerId,
        long teamId);
}

public interface ILineupSyncService
{
    Task<LineupClean?> SyncAsync(
        MatchDetailSnapshot snapshot);
}

public interface ILineupPlayerSyncService
{
    Task SyncAsync(
        MatchDetailSnapshot snapshot,
        long lineupId);
}

public interface ILineupBackfillService
{
    Task SyncMissingAsync(int teamId);
}