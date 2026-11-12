using FootballApi.Repositories.Lineup;
using FootballApi.Repositories.Match;
using FootballApi.Repositories.MatchEvent;
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

public class LineupRepository : ILineupRepository
{
    private readonly Supabase.Client _client;
    private readonly IMatchEventRepository _matchEventRepository;

    public LineupRepository(
        SupabaseClientFactory factory,
        IMatchEventRepository matchEventRepository)
    {
        _client = factory.CreateAnonClient();
        _matchEventRepository = matchEventRepository;
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
        var offset = (page - 1) * pageSize;

        // Đếm và lấy trang trên CÙNG view => sort theo match_date đúng trên toàn bộ dữ liệu, không chỉ trong 1 trang.
        var totalCount = await _client.From<PlayerAppearanceViewClean>()
            .Where(x => x.PlayerId == playerId)
            .Count(Supabase.Postgrest.Constants.CountType.Exact, ct);

        if (totalCount == 0 || offset >= totalCount)
            return (new List<PlayerAppearanceRecord>(), totalCount);

        var page_ = await _client.From<PlayerAppearanceViewClean>()
            .Where(x => x.PlayerId == playerId)
            // Thứ tự xác định: ngày trận, rồi match_id, rồi id dòng làm tie-break => paging không trùng/sót.
            .Order(x => x.MatchDate, Supabase.Postgrest.Constants.Ordering.Descending)
            .Order(x => x.MatchId, Supabase.Postgrest.Constants.Ordering.Descending)
            .Order(x => x.LineupPlayerId, Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(offset, offset + pageSize - 1)
            .Get(ct);

        var rows = page_.Models;
        if (rows.Count == 0)
            return (new List<PlayerAppearanceRecord>(), totalCount);

        var matchIds = rows.Select(r => r.MatchId).Distinct().ToList();

        // Goals: chỉ event_type == "goal" (own_goal không tính). Assists theo AssistPlayerId.
        var events = await _matchEventRepository.GetEventsByMatchIdsAsync(matchIds, ct);
        var goalsByMatch = events
            .Where(e => e.EventType == "goal" && e.PlayerId == playerId)
            .GroupBy(e => e.MatchId)
            .ToDictionary(g => g.Key, g => g.Count());
        var assistsByMatch = events
            .Where(e => e.AssistPlayerId == playerId)
            .GroupBy(e => e.MatchId)
            .ToDictionary(g => g.Key, g => g.Count());

        var items = rows
            .Select(r => r.ToRecord(
                goalsByMatch.GetValueOrDefault(r.MatchId, 0),
                assistsByMatch.GetValueOrDefault(r.MatchId, 0)))
            .ToList();

        return (items, totalCount);
    }

}