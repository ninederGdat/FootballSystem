using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FootballSystem.Shared.Players;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;

namespace FootballApi.Repositories.Player;

public class PlayerRepository : IPlayerRepository
{
    private readonly Supabase.Client _client;

    public PlayerRepository(SupabaseClientFactory factory) => _client = factory.CreateAnonClient();

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
            .Filter("player_id", Constants.Operator.In, ids)
            .Get();
        return result.Models;
    }

    public async Task<(List<PlayerClean> Items, int TotalCount)> SearchAsync(
    string? search,
    long teamId,
    string? positionCode,
    string? nationality,
    string? status,
    PlayerRelation? relation,
    int page,
    int pageSize,
    CancellationToken ct)
    {
        var query = _client.From<PlayerClean>();
        var x = teamId;

        switch (relation)
        {
            case PlayerRelation.Current:
                query.Filter("current_team_id", Constants.Operator.Equals, x);
                query.Or(new List<IPostgrestQueryFilter>
            {
                new QueryFilter("transfer_status", Constants.Operator.NotEqual, "loaned"),
                new QueryFilter("team_id", Constants.Operator.Equals, x)
            });
                break;

            case PlayerRelation.LoanedIn:
                query.Filter("current_team_id", Constants.Operator.Equals, x);
                query.Filter("team_id", Constants.Operator.NotEqual, x);
                query.Filter("transfer_status", Constants.Operator.Equals, "loaned");
                break;

            case PlayerRelation.LoanedOut:
                query.Filter("team_id", Constants.Operator.Equals, x);
                query.Filter("current_team_id", Constants.Operator.NotEqual, x);
                query.Filter("transfer_status", Constants.Operator.Equals, "loaned");
                break;

            case PlayerRelation.Transferred:
                query.Filter("team_id", Constants.Operator.Equals, x);
                query.Filter("current_team_id", Constants.Operator.NotEqual, x);
                query.Filter("transfer_status", Constants.Operator.NotEqual, "loaned");
                break;

            default: // mặc định: liên quan tới X và loại Transferred
                query.Or(new List<IPostgrestQueryFilter>
            {
                new QueryFilter("team_id", Constants.Operator.Equals, x),
                new QueryFilter("current_team_id", Constants.Operator.Equals, x)
            });
                query.Or(new List<IPostgrestQueryFilter>
            {
                new QueryFilter("current_team_id", Constants.Operator.Equals, x),
                new QueryFilter("transfer_status", Constants.Operator.Equals, "loaned")
            });
                break;
        }

        // Filter<bool> không hỗ trợ -> dùng chuỗi
        query.Filter("is_stub", Constants.Operator.Equals, "false");

        if (!string.IsNullOrWhiteSpace(positionCode))
            query.Filter("preferred_position_code", Constants.Operator.Equals, positionCode);

        if (!string.IsNullOrWhiteSpace(nationality))
            query.Filter("nationality", Constants.Operator.Equals, nationality);

        if (!string.IsNullOrWhiteSpace(status))
            query.Filter("status", Constants.Operator.Equals, status);

        if (!string.IsNullOrWhiteSpace(search))
            query.Filter("name", Constants.Operator.ILike, $"%{search}%");

        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : pageSize;

        // Số áo tăng dần (null cuối), rồi player_id làm tie-breaker để phân trang ổn định
        query.Order(p => p.ShirtNumber, Constants.Ordering.Ascending, Constants.NullPosition.Last);
        query.Order(p => p.PlayerId, Constants.Ordering.Ascending, Constants.NullPosition.Last);

        var offset = (safePage - 1) * safePageSize;
        query.Range(offset, offset + safePageSize - 1);

        var result = await query.Get(ct, Constants.CountType.Exact);
        return (result.Models, result.Count);
    }



}