using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Player;

public class PlayerRepository : IPlayerRepository
{
    private readonly Supabase.Client _client;

    public PlayerRepository(SupabaseClientFactory factory) => _client = factory.CreateServiceRoleClient();

    public async Task<PlayerClean?> GetByIdAsync(long playerId, CancellationToken ct)
    {
        var result = await _client.From<PlayerClean>()
            .Where(x => x.PlayerId == playerId)
            .Get();
        return result.Models.FirstOrDefault();
    }

    public async Task<List<PlayerClean>> GetByIdsAsync(IEnumerable<long> playerIds, CancellationToken ct)
    {
        var ids = playerIds.ToList();
        if (ids.Count == 0) return [];

        var result = await _client.From<PlayerClean>()
            .Filter("player_id", Supabase.Postgrest.Constants.Operator.In, ids)
            .Get();
        return result.Models;
    }

    public async Task<(List<PlayerClean> Items, int TotalCount)> SearchAsync(
        string? search, long? teamId, string? positionCode, string? nationality,
        int page, int pageSize, CancellationToken ct)
    {
        // Postgrest query builder — filter động, chỉ áp field nào có giá trị.
        var query = _client.From<PlayerClean>();

        // Use .Filter returns IPostgrestTable, not ISupabaseTable, so avoid reassignment
        if (teamId is not null)
            query.Filter("team_id", Supabase.Postgrest.Constants.Operator.Equals, teamId.Value);

        if (!string.IsNullOrWhiteSpace(positionCode))
            query.Filter("preferred_position_code", Supabase.Postgrest.Constants.Operator.Equals, positionCode);

        if (!string.IsNullOrWhiteSpace(nationality))
            query.Filter("nationality", Supabase.Postgrest.Constants.Operator.Equals, nationality);

        if (!string.IsNullOrWhiteSpace(search))
            query.Filter("name", Supabase.Postgrest.Constants.Operator.ILike, $"%{search}%");

        // NOTE: Postgrest .Count() cần 1 request riêng để lấy tổng số bản ghi (không tính từ page hiện tại).
        // Nếu Supabase.Client bản bạn dùng hỗ trợ .Count(Supabase.Postgrest.Constants.CountType.Exact),
        // gọi riêng trước khi .Range() để lấy TotalCount chính xác. Tạm thời mình để verify lại API version:
        var countResult = await query.Count(Supabase.Postgrest.Constants.CountType.Exact);

        var offset = (page - 1) * pageSize;
        var pagedResult = await query
            .Range(offset, offset + pageSize - 1)
            .Get();

        return (pagedResult.Models, countResult);
    }


    
}