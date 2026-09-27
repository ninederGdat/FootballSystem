using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Competition;

public class CompetitionRepository : ICompetitionRepository
{
    private readonly Supabase.Client _client;

    public CompetitionRepository(SupabaseClientFactory factory)
    {
        _client = factory.CreateAnonClient();
    }

    public async Task<IReadOnlyList<CompetitionClean>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _client
            .From<CompetitionClean>()
            .Order("name", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get(ct);

        return result.Models ?? [];
    }
}
