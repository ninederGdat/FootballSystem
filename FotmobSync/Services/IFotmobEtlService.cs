using FootballSystem.Shared.Models.Clean;
using FotmobSync.Infrastructure.Resolvers.TransferStatus;
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
    Task SyncAsync(TeamDataSnapshot snapshot, CancellationToken cancellationToken = default);
    Task SyncAsync(IReadOnlyCollection<TeamDataSnapshot> snapshots, CancellationToken cancellationToken = default);
}

public interface IPlayerSyncService
{
    Task<bool> SyncAsync(
        long playerId,
        long teamId,
         bool useProfileTeam = false,
        CancellationToken cancellationToken = default);

    Task EnrichAsync(int batchSize = 10, CancellationToken cancellationToken = default);

}

public interface ILineupSyncService
{
    Task<LineupClean?> SyncAsync(
        MatchDetailSnapshot snapshot);

    Task MarkCompleteAsync(long lineupId);
}


public interface ILineupPlayerSyncService
{
    Task<LineupPlayerSyncResult> SyncAsync(
        MatchDetailSnapshot snapshot,
        long lineupId);
}

public interface ILineupBackfillService
{
    Task SyncMissingAsync(int teamId);
}

public interface IMatchEventSyncService
{
    Task SyncAsync(
        MatchDetailSnapshot snapshot,
        long matchId);
}

public interface ITransferSyncService
{
    Task SyncAsync(TeamDataSnapshot snapshot, CancellationToken cancellationToken = default);
    Task SyncAsync(IReadOnlyCollection<TeamDataSnapshot> snapshots, CancellationToken cancellationToken = default);
}

public interface ITransferStatusSyncService
{
    Task SyncAsync(
        IEnumerable<PlayerTransferStatus> statuses,
        CancellationToken cancellationToken = default);
}

public interface IPlayerStubService
{
    Task<int> EnsureAsync(IEnumerable<PlayerStubInput> inputs, long teamId);
}
