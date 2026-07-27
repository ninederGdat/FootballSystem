using FootballApi.Repositories.Lineup;
using FootballApi.Repositories.Match;
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

public class LineupRepository : ILineupRepository
{
    private readonly Supabase.Client _client;
    private readonly IMatchRepository _matchRepository;

    public LineupRepository(SupabaseClientFactory factory, IMatchRepository matchRepository)
    {
        _client = factory.CreateServiceRoleClient();
        _matchRepository = matchRepository;
    }

    public async Task<LineupClean?> GetLineupByMatchIdAsync(long matchId, CancellationToken ct = default)
    {
        var result = await _client.From<LineupClean>()
            .Where(x => x.MatchId == matchId)
            .Get(ct);
        return result.Models.FirstOrDefault();
    }

    public async Task<FormationClean?> GetFormationByIdAsync(long formationId, CancellationToken ct = default)
    {
        var result = await _client.From<FormationClean>()
            .Where(x => x.Id == formationId)
            .Get(ct);
        return result.Models.FirstOrDefault();
    }

    public async Task<List<LineupPlayerClean>> GetLineupPlayersAsync(long lineupId, CancellationToken ct = default)
    {
        var result = await _client.From<LineupPlayerClean>()
            .Where(x => x.LineupId == lineupId)
            .Get(ct);
        return result.Models;
    }

    public async Task<List<PlayerClean>> GetPlayersByIdsAsync(List<long> playerIds, CancellationToken ct = default)
    {
        var result = await _client.From<PlayerClean>()
            .Filter("player_id", Supabase.Postgrest.Constants.Operator.In, playerIds)
            .Get(ct);
        return result.Models;
    }

    public async Task<List<PositionClean>> GetPositionsByCodesAsync(List<string> positionCodes, CancellationToken ct = default)
    {
        var result = await _client.From<PositionClean>()
            .Filter("position_code", Supabase.Postgrest.Constants.Operator.In, positionCodes)
            .Get(ct);
        return result.Models;
    }

    public async Task<List<PositionRoleClean>> GetPositionRolesByIdsAsync(List<int> roleIds, CancellationToken ct = default)
    {
        var result = await _client.From<PositionRoleClean>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, roleIds)
            .Get(ct);
        return result.Models;
    }

    public async Task<(List<LineupPlayerClean> Items, int TotalCount)> GetLineupPlayersByPlayerIdAsync(
        long playerId, int page, int pageSize, CancellationToken ct = default)
    {
        var offset = (page - 1) * pageSize;

        var countResult = await _client.From<LineupPlayerClean>()
            .Where(x => x.PlayerId == playerId)
            .Count(Supabase.Postgrest.Constants.CountType.Exact, ct);

        var result = await _client.From<LineupPlayerClean>()
            .Where(x => x.PlayerId == playerId)
            // NOTE: sort theo LineupId làm proxy cho thời gian vì Match chưa được join tại bước này.
            // Đây là known limitation: thứ tự CHÍNH XÁC theo MatchDate chỉ đảm bảo trong phạm vi 1 trang,
            // không đảm bảo giữa các trang. Nếu cần chính xác tuyệt đối, phải join trước khi phân trang (view/RPC).
            .Order(x => x.LineupId, Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(offset, offset + pageSize - 1)
            .Get(ct);

        return (result.Models, countResult);
    }

    public async Task<List<LineupClean>> GetByIdsAsync(List<long> lineupIds, CancellationToken ct = default)
    {
        if (lineupIds.Count == 0) return [];
        var result = await _client.From<LineupClean>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, lineupIds)
            .Get(ct);
        return result.Models;
    }

    // ---------------------------------------------------------------
    // Đóng gói việc "đi theo FK": lineup_players -> lineups -> matches.
    // Service gọi 1 method duy nhất, không cần biết cấu trúc join bên trong.
    // ---------------------------------------------------------------
  public async Task<(List<PlayerAppearanceRecord> Items, int TotalCount)> GetPlayerAppearancesAsync(
    long playerId, int page, int pageSize, CancellationToken ct = default)
{
    var (pageItems, totalCount) = await GetLineupPlayersByPlayerIdAsync(playerId, page, pageSize, ct);

    if (pageItems.Count == 0)
    {
        return (new List<PlayerAppearanceRecord>(), totalCount);
    }

    var lineupIds = pageItems
        .Where(i => i.LineupId.HasValue)
        .Select(i => i.LineupId!.Value)
        .Distinct()
        .ToList();

    var lineups = await GetByIdsAsync(lineupIds, ct);
    var lineupsById = lineups.ToDictionary(l => l.Id, l => l);

    var matchIds = lineups.Select(l => l.MatchId).Distinct().ToList();
    var matches = await _matchRepository.GetByMatchIdsAsync(matchIds, ct); // <-- tên đúng
var matchesById = matches
    .GroupBy(m => m.MatchId)
    .ToDictionary(g => g.Key, g => g.First());   

    var items = pageItems.Select(lp =>
    {
        var lineup = lp.LineupId.HasValue ? lineupsById.GetValueOrDefault(lp.LineupId.Value) : null;
        var match = lineup is not null ? matchesById.GetValueOrDefault(lineup.MatchId) : null;

        return new PlayerAppearanceRecord
        {
            MatchId = match?.MatchId ?? 0,
            MatchDate = match?.MatchDate ?? default,
            OpponentName = match?.OpponentName,
            CompetitionName = match?.CompetitionName,
            IsStarter = lp.IsStarter,
            PositionCode = lp.PositionCode,
            RoleId = lp.RoleId,
            MinuteIn = lp.MinuteIn,
            MinuteOut = lp.MinuteOut
        };
    }).ToList();

    return (items, totalCount);
}
}