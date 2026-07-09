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

    public async Task<MatchClean?> GetByMatchIdAsync(long matchId)
    {
        var response = await _client
            .From<MatchClean>()
            .Filter("match_id", Constants.Operator.Equals, matchId)
            .Get();

        var models = response.Models ?? [];

        if (models.Count > 1)
        {
            _logger.LogWarning(
                "Phát hiện {Count} hàng cho match_id {MatchId} — hệ thống hiện giả định 1 hàng/trận. Cần bổ sung logic chọn góc nhìn team.",
                models.Count, matchId);
        }

        return models.FirstOrDefault();
    }
}