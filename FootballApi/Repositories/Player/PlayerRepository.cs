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
        string? search,
        long? teamId,
        string? positionCode,
        string? nationality,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = _client.From<PlayerClean>();

        if (teamId is not null)
            query.Filter(
                "team_id",
                Supabase.Postgrest.Constants.Operator.Equals,
                teamId.Value);

        if (!string.IsNullOrWhiteSpace(positionCode))
            query.Filter(
                "preferred_position_code",
                Supabase.Postgrest.Constants.Operator.Equals,
                positionCode);

        if (!string.IsNullOrWhiteSpace(nationality))
            query.Filter(
                "nationality",
                Supabase.Postgrest.Constants.Operator.Equals,
                nationality);

        if (!string.IsNullOrWhiteSpace(search))
            query.Filter(
                "name",
                Supabase.Postgrest.Constants.Operator.ILike,
                $"%{search}%");

        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : pageSize;

        // Order by shirt number ascending, nulls last (players without a shirt number will appear at the end of the list)
          query.Order(x => x.ShirtNumber, Supabase.Postgrest.Constants.Ordering.Ascending, Supabase.Postgrest.Constants.NullPosition.Last);

        var offset = (safePage - 1) * safePageSize;
        var limit = offset + safePageSize - 1;

        query.Range(offset, limit);

        var result = await query.Get(ct, Supabase.Postgrest.Constants.CountType.Exact);

        return (result.Models, result.Count);
    }



}