// Repositories/PositionRole/PositionRoleRepository.cs
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.PositionRole;

public class PositionRoleRepository : IPositionRoleRepository
{
    private readonly Supabase.Client _client;

    public PositionRoleRepository(SupabaseClientFactory factory) => _client = factory.CreateServiceRoleClient();

    public async Task<List<PositionRoleClean>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _client.From<PositionRoleClean>().Get(ct);
        return result.Models;
    }

    public async Task<List<PositionRoleClean>> GetByIdsAsync(List<int> roleIds, CancellationToken ct = default)
    {
        if (roleIds.Count == 0) return [];
        var result = await _client.From<PositionRoleClean>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, roleIds)
            .Get(ct);
        return result.Models;
    }
}