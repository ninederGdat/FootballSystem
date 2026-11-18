using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.SyncRun;

public interface ISyncRunRepository
{
    /// <summary>Most recent official (GitHub) runs, newest first.</summary>
    Task<IReadOnlyList<SyncRunClean>> GetRecentRunsAsync(int limit, CancellationToken ct = default);

    /// <summary>Phases with status 'failed' for the given runs.</summary>
    Task<IReadOnlyList<SyncRunPhaseClean>> GetFailedPhasesAsync(
        IReadOnlyCollection<long> runIds, CancellationToken ct = default);
}