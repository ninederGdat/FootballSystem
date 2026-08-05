using System.Text.Json;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Models.Raw;

namespace FotmobSync.Mappers;

/// <summary>
/// Chuyển đổi MatchEventRaw (content.matchFacts.events.events[]) thành MatchEventUpsert.
/// Theo pattern Raw -> Mapper -> Clean/Upsert, cùng convention static/extension
/// như <see cref="MatchMapper"/>, <see cref="LineupMapper"/>.

/// </summary>
public static class MatchEventMapper
{
    /// <param name="raw">Event đã deserialize, có SourceIndex và RawJson đã gán.</param>
    /// <param name="matchId">Match nội bộ (DB).</param>
    /// <param name="knownPlayerIds">Tập player_id (Fotmob) đã tồn tại trong DB players.</param>
    /// <returns>Null nếu event type ngoài phạm vi (Goal/Card) hoặc raw null.</returns>

    public static MatchEventUpsert? ToUpsert(
        this MatchEventRaw? raw,
        long matchId,
        long homeTeamId,
        long awayTeamId,
        long followedTeamId,
        IReadOnlySet<long> knownPlayerIds)
    {
        if (raw is null)
            return null;

        var eventType = ResolveEventType(raw);
        if (eventType is null)
            return null;

        var actualTeamId = raw.IsHome ? homeTeamId : awayTeamId;

        // Chỉ đội đang theo dõi mới có row trong bảng teams.
        // Kiểm tra khớp trước khi gán, nếu không thì để NULL thay vì
        // insert giá trị vi phạm FK.
        long? teamId = actualTeamId == followedTeamId
            ? actualTeamId
            : null;

        long? playerId = ResolvePlayerId(raw.Player?.Id, knownPlayerIds);
        long? assistPlayerId = ResolvePlayerId(raw.AssistPlayerId, knownPlayerIds);

        
        

        return new MatchEventUpsert
        {
            MatchId = matchId,
            FotmobEventId = raw.EventId,
            EventType = eventType,
            EventOrder = raw.SourceIndex,
            Minute = raw.Time,
            StoppageTime = raw.OverloadTime,
            TeamId = teamId,
            PlayerId = playerId,
            AssistPlayerId = assistPlayerId,
            DescriptionKey = raw.GoalDescriptionKey,
            RawPayload = raw.RawJson.GetRawText(),
        };
    }

    /// <summary>
    /// Convert danh sách raw events → danh sách upsert.
    /// Event ngoài phạm vi (type không xác định) bị lọc bỏ lặng lẽ;
    /// caller (Service) tự log dựa trên chênh lệch count nếu cần.
    /// </summary>
    public static List<MatchEventUpsert> ToUpsertList(
        this IEnumerable<MatchEventRaw> rawEvents,
        long matchId,
        long homeTeamId,
        long awayTeamId,
        long followedTeamId,
        IReadOnlySet<long> knownPlayerIds)
    {
        return rawEvents
            .Select(r => r.ToUpsert(matchId, homeTeamId, awayTeamId, followedTeamId, knownPlayerIds))
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();
    }

    /// <summary>
    /// "Goal" + ownGoal -> own_goal | goal
    /// "Card" + card    -> yellow | yellow_red | red
    /// Type khác        -> null (bỏ qua, out of scope hiện tại)
    /// </summary>
    private static string? ResolveEventType(MatchEventRaw raw)
    {
        return raw.Type switch
        {
            "Goal" => raw.OwnGoal == true ? "own_goal" : "goal",
            "Card" => raw.Card switch
            {
                "Yellow" => "yellow",
                "YellowRed" => "yellow_red",
                "Red" => "red",
                _ => null,
            },
            _ => null,
        };
    }

    private static long? ResolvePlayerId(long? fotmobPlayerId, IReadOnlySet<long> knownPlayerIds)
    {
        if (fotmobPlayerId is null)
            return null;

        return knownPlayerIds.Contains(fotmobPlayerId.Value) ? fotmobPlayerId : null;
    }
}