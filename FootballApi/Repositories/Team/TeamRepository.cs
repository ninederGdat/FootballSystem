// Repositories/Team/TeamRepository.cs
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Team;

public class TeamRepository : ITeamRepository
{
    private readonly Supabase.Client _client;

    public TeamRepository(SupabaseClientFactory factory) => _client = factory.CreateServiceRoleClient();

    public async Task<TeamClean?> GetByIdAsync(long teamId, CancellationToken ct = default)
    {
        var result = await _client.From<TeamClean>()
            .Where(x => x.TeamId == teamId)
            .Get(ct);
        return result.Models.FirstOrDefault();
    }

    public async Task<List<TeamClean>> GetByIdsAsync(List<long> teamIds, CancellationToken ct = default)
    {
        if (teamIds.Count == 0) return [];
        var result = await _client.From<TeamClean>()
            .Filter("team_id", Supabase.Postgrest.Constants.Operator.In, teamIds)
            .Get(ct);
        return result.Models;
    }
}
