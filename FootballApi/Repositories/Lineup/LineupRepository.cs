using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

public class LineupRepository : ILineupRepository
{
    private readonly Supabase.Client _client;

    public LineupRepository(SupabaseClientFactory factory) => _client = factory.CreateServiceRoleClient();

    public async Task<LineupClean?> GetLineupByMatchIdAsync(long matchId)
    {
        var result = await _client.From<LineupClean>()
            .Where(x => x.MatchId == matchId)
            .Get();
        return result.Models.FirstOrDefault();
    }

    public async Task<FormationClean?> GetFormationByIdAsync(long formationId)
    {
        var result = await _client.From<FormationClean>()
            .Where(x => x.Id == formationId)
            .Get();
        return result.Models.FirstOrDefault();
    }

    public async Task<List<LineupPlayerClean>> GetLineupPlayersAsync(long lineupId)
    {
        var result = await _client.From<LineupPlayerClean>()
            .Where(x => x.LineupId == lineupId)
            .Get();
        return result.Models;
    }

    public async Task<List<PlayerClean>> GetPlayersByIdsAsync(List<long> playerIds)
    {
        var result = await _client.From<PlayerClean>()
            .Filter("player_id", Supabase.Postgrest.Constants.Operator.In, playerIds)
            .Get();
        return result.Models;
    }

    public async Task<List<PositionClean>> GetPositionsByCodesAsync(List<string> positionCodes)
    {
        var result = await _client.From<PositionClean>()
            .Filter("position_code", Supabase.Postgrest.Constants.Operator.In, positionCodes)
            .Get();
        return result.Models;
    }

    public async Task<List<PositionRoleClean>> GetPositionRolesByIdsAsync(List<int> roleIds)
    {
        var result = await _client.From<PositionRoleClean>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, roleIds)
            .Get();
        return result.Models;
    }
}