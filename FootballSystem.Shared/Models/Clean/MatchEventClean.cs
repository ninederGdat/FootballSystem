namespace FootballSystem.Shared.Models.Clean;

/// <summary>
/// Model DB-shape cho bảng match_events. Map 1-1 với schema (xem migration 001_create_match_events.sql).
/// Không có IsPenalty riêng — lọc phạt đền bằng DescriptionKey == "penalty" ở tầng query.
/// </summary>
public class MatchEventClean
{
    public long Id { get; set; }
    public long MatchId { get; set; }
    public long FotmobEventId { get; set; }

    /// <summary>goal | own_goal | yellow | yellow_red | red (snake_case, gần với Fotmob gốc).
    /// Không ràng buộc enum ở DB — validate ở Mapper.</summary>
    public string EventType { get; set; } = default!;

    /// <summary>Index gốc trong events[] — dùng để ORDER BY Minute, EventOrder cho đúng timeline
    /// khi nhiều event xảy ra cùng phút.</summary>
    public int EventOrder { get; set; }

    public int Minute { get; set; }
    public int? StoppageTime { get; set; }

    public long TeamId { get; set; }
    public long? PlayerId { get; set; }
    public long? AssistPlayerId { get; set; }

    /// <summary>goalDescriptionKey gốc (overhead_kick, penalty, header...), null cho event Card.</summary>
    public string? DescriptionKey { get; set; }

    /// <summary>Toàn bộ object event gốc từ Fotmob, dạng JSON string.</summary>
    public string RawPayload { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}