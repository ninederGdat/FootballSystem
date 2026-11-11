using FotmobSync.Models.Raw;

namespace FotmobSync.Mappers;

/// <summary>
/// Đọc điểm luân lưu từ event type "PenaltyShootout" (content.matchFacts.events.events[]).
/// Fotmob chỉ gửi một event tóm tắt, penaltyScore = [chủ nhà, khách]; không có từng quả đá.
/// </summary>
public static class PenaltyShootoutMapper
{
    public const string EventType = "PenaltyShootout";

    /// <returns>Null nếu trận không có luân lưu hoặc penaltyScore không hợp lệ.</returns>
    public static (int Home, int Away)? TryGetScore(this IEnumerable<MatchEventRaw> events)
    {
        var shootout = events.LastOrDefault(e =>
            string.Equals(e.Type, EventType, StringComparison.Ordinal)
            && e.PenaltyScore is { Count: 2 }
            && e.PenaltyScore.All(s => s >= 0));

        return shootout is null
            ? null
            : (shootout.PenaltyScore![0], shootout.PenaltyScore[1]);
    }
}