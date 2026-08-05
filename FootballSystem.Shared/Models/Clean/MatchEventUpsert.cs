using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

/// <summary>
/// Model tối giản cho upsert match_events.
/// Khóa unique dùng để upsert: fotmob_event_id.
/// Không map is_penalty riêng (dùng description_key == "penalty").
/// </summary>
[Table("match_events")]
public class MatchEventUpsert : BaseModel
{
    [Column("match_id")]
    public long MatchId { get; set; }

    [Column("fotmob_event_id")]
    public long FotmobEventId { get; set; }

    // snake_case gần tên gốc Fotmob: goal, own_goal, yellow, yellow_red, red
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    // Index gốc trong content.matchFacts.events.events[]
    // dùng ORDER BY minute, event_order để tái dựng đúng timeline.
    [Column("event_order")]
    public int EventOrder { get; set; }

    [Column("minute")]
    public int? Minute { get; set; }

    [Column("stoppage_time")]
    public int? StoppageTime { get; set; }

    // NULL khi là đội khách (không tồn tại trong bảng teams, đúng phạm vi single-team).
    // Tên/id gốc vẫn giữ được qua raw_payload.
    [Column("team_id")]
    public long? TeamId { get; set; }

    // NULL khi player không tồn tại trong DB (vd. cầu thủ đối phương, theo quyết định
    // không sync tối thiểu). Tên gốc vẫn giữ được qua raw_payload.
    [Column("player_id")]
    public long? PlayerId { get; set; }

    [Column("assist_player_id")]
    public long? AssistPlayerId { get; set; }

    // vd. "penalty" — không dùng cột is_penalty riêng.
    [Column("description_key")]
    public string? DescriptionKey { get; set; }

    [Column("raw_payload")]
    public string RawPayload { get; set; } = "{}";
}