using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

/// <summary>
/// Model DB-shape cho bảng match_events. Map 1-1 với schema (xem migration 001_create_match_events.sql).
/// Không có IsPenalty riêng — lọc phạt đền bằng DescriptionKey == "penalty" ở tầng query.
/// </summary>
[Table("match_events")]

public class MatchEventClean : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }
    [Column("match_id")]
    public long MatchId { get; set; }
    [Column("fotmob_event_id")]
    public long FotmobEventId { get; set; }

    /// <summary>goal | own_goal | yellow | yellow_red | red (snake_case, gần với Fotmob gốc).
    /// Không ràng buộc enum ở DB — validate ở Mapper.</summary>
    [Column("event_type")]
    public string EventType { get; set; } = default!;

    /// <summary>Index gốc trong events[] — dùng để ORDER BY Minute, EventOrder cho đúng timeline
    /// khi nhiều event xảy ra cùng phút.</summary>
    [Column("event_order")]
    public int EventOrder { get; set; }
    [Column("minute")]
    public int Minute { get; set; }
    [Column("stoppage_time")]
    public int? StoppageTime { get; set; }
    [Column("team_id")]
    public long? TeamId { get; set; }
    [Column("player_id")]
    public long? PlayerId { get; set; }
    [Column("assist_player_id")]
    public long? AssistPlayerId { get; set; }

    /// <summary>goalDescriptionKey gốc (overhead_kick, penalty, header...), null cho event Card.</summary>
    [Column("description_key")]
    public string? DescriptionKey { get; set; }

    /// <summary>Toàn bộ object event gốc từ Fotmob, dạng JSON string.</summary>
    [Column("raw_payload")]
    public string RawPayload { get; set; } = default!;
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("last_updated")]
    public DateTime LastUpdated { get; set; }
}