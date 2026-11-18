using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using Supabase.Postgrest;

namespace FootballApi.Repositories.SyncRun;

public class SyncRunRepository : ISyncRunRepository
{
    // Runs started by GitHub Actions (see .github/workflows/fotmob-sync.yml).
    // Local and Quartz runs have trigger = null and are intentionally excluded.
    private static readonly List<string> OfficialTriggers = ["schedule", "workflow_dispatch"];

    private readonly Supabase.Client _client;

    public SyncRunRepository(SupabaseClientFactory factory)
    {
        _client = factory.CreateAnonClient();
    }

    public async Task<IReadOnlyList<SyncRunClean>> GetRecentRunsAsync(int limit, CancellationToken ct = default)
    {
        var result = await _client.From<SyncRunClean>()
            .Filter("trigger", Constants.Operator.In, OfficialTriggers)
            .Order("started_at", Constants.Ordering.Descending)
            .Order("id", Constants.Ordering.Descending)
            .Limit(limit)
            .Get(ct);

        return result.Models ?? [];
    }

    public async Task<IReadOnlyList<SyncRunPhaseClean>> GetFailedPhasesAsync(
        IReadOnlyCollection<long> runIds, CancellationToken ct = default)
    {
        if (runIds.Count == 0) return [];

        var result = await _client.From<SyncRunPhaseClean>()
            .Filter("run_id", Constants.Operator.In, runIds.ToList())
            .Filter("status", Constants.Operator.Equals, "failed")
            .Order("run_id", Constants.Ordering.Descending)
            .Order("phase_order", Constants.Ordering.Ascending)
            .Get(ct);

        return result.Models ?? [];
    }
}