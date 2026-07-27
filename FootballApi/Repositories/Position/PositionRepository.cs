using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Position;

public class PositionRepository : IPositionRepository
{
    private readonly Supabase.Client _client;

    public PositionRepository(SupabaseClientFactory factory) => _client = factory.CreateServiceRoleClient();

    public async Task<List<PositionClean>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _client.From<PositionClean>().Get(ct);
        return result.Models;
    }

    public async Task<List<PositionClean>> GetByCodesAsync(List<string> positionCodes, CancellationToken ct = default)
    {
        if (positionCodes.Count == 0) return [];
        var result = await _client.From<PositionClean>()
            .Filter("position_code", Supabase.Postgrest.Constants.Operator.In, positionCodes)
            .Get(ct);
        return result.Models;
    }
}