using FootballApi.Repositories.Match;
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using Supabase.Postgrest;

namespace FootballApi.Repositories.Match;

public class MatchRepository : IMatchRepository
{
    private readonly Supabase.Client _client;
    private readonly ILogger<MatchRepository> _logger;

    public MatchRepository(SupabaseClientFactory factory, ILogger<MatchRepository> logger)
    {
        _client = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task<MatchClean?> GetByMatchIdAsync(long matchId, CancellationToken ct = default)
    {
        var response = await _client
            .From<MatchClean>()
            .Filter("match_id", Constants.Operator.Equals, matchId)
            .Get(ct);

        var models = response.Models ?? [];

        if (models.Count > 1)
        {
            _logger.LogWarning(
                "Phát hiện {Count} hàng cho match_id {MatchId} — hệ thống hiện giả định 1 hàng/trận. Cần bổ sung logic chọn góc nhìn team.",
                models.Count, matchId);
        }

        return models.FirstOrDefault();
    }

    public async Task<List<MatchClean>> GetByMatchIdsAsync(IEnumerable<long> matchIds, CancellationToken ct = default)
    {
        var ids = matchIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        var response = await _client
            .From<MatchClean>()
            .Filter("match_id", Constants.Operator.In, ids)
            .Get(ct);

        return response.Models ?? [];
    }

    public async Task<(List<MatchClean> Items, int TotalCount)> SearchAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? opponent,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {

        var query = _client.From<MatchClean>();

        if (fromDate is not null)
        {
            query.Filter(
                "match_date",
                Constants.Operator.GreaterThanOrEqual,
                fromDate.Value.ToString("yyyy-MM-dd")
            );
        }

        if (toDate is not null)
        {
            query.Filter(
                "match_date",
                Constants.Operator.LessThan,
                toDate.Value.ToString("yyyy-MM-dd")
            );
        }

        if (!string.IsNullOrWhiteSpace(opponent))
            query.Filter(
                "opponent_name",
                Constants.Operator.Like,
                $"%{opponent}%"
            );

        if (!string.IsNullOrWhiteSpace(status))
            query.Filter(
                "status",
                Constants.Operator.Equals,
                status);

        // Most recent/soonest matches first — matches the fan-facing "what's happening" use case.
        query.Order("match_date", Constants.Ordering.Ascending);

        // Clamp to sane values so a bad/zero pageSize can't blow up Range() or return everything.
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : pageSize;

        var offset = (safePage - 1) * safePageSize;
        var limit = offset + safePageSize - 1;

        query.Range(offset, limit);

        // CountType.Exact makes Postgrest add `Prefer: count=exact`, which populates
        // result.Count from the response's Content-Range header.
        var result = await query.Get(ct, Constants.CountType.Exact);

        return (result.Models, result.Count);
    }

}