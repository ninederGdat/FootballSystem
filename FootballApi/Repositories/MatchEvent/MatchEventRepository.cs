using FootballApi.Repositories.MatchEvent;
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

public class MatchEventRepository : IMatchEventRepository
{
    private readonly Supabase.Client _client;

    public MatchEventRepository(SupabaseClientFactory factory)
    {
        _client = factory.CreateServiceRoleClient();
    }

    public async Task<List<MatchEventClean>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default)
    {
        var result = await _client.From<MatchEventClean>()
            .Where(x => x.MatchId == matchId)
            .Order(x => x.Minute, Supabase.Postgrest.Constants.Ordering.Ascending)
            .Order(x => x.EventOrder, Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get(ct);
        return result.Models;
    }
}