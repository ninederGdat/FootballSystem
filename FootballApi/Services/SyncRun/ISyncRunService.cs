using FootballApi.DTOs.Sync;

namespace FootballApi.Services.SyncRun;

public interface ISyncRunService
{
    Task<IReadOnlyList<SyncRunResponse>> GetRecentAsync(int? limit, CancellationToken ct = default);
}