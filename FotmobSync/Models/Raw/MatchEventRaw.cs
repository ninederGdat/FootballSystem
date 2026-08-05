using System.Text.Json;
using System.Text.Json.Serialization;

namespace FotmobSync.Models.Raw;

/// <summary>
/// Raw model cho 1 phần tử trong content.matchFacts.events.events[].
/// Map 1-1 các field cần cho Mapper; toàn bộ JSON gốc vẫn được giữ qua RawJson.
/// </summary>
public class MatchEventRaw
{
    [JsonPropertyName("eventId")]
    public long EventId { get; set; }

    /// <summary>"Goal" hoặc "Card". Các type khác (nếu Fotmob trả) vẫn được
    /// giữ nguyên trong RawJson dù Mapper hiện tại chưa xử lý.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("overloadTime")]
    public int? OverloadTime { get; set; }

    [JsonPropertyName("isHome")]
    public bool IsHome { get; set; }

    [JsonPropertyName("player")]
    public MatchEventPlayerRaw? Player { get; set; }

    // --- Goal-specific (nullable vì Card event không có các field này) ---

    [JsonPropertyName("ownGoal")]
    public bool? OwnGoal { get; set; }

    [JsonPropertyName("goalDescriptionKey")]
    public string? GoalDescriptionKey { get; set; }

    [JsonPropertyName("isPenaltyShootoutEvent")]
    public bool IsPenaltyShootoutEvent { get; set; }

    [JsonPropertyName("assistPlayerId")]
    public long? AssistPlayerId { get; set; }

    // --- Card-specific (nullable vì Goal event không có các field này) ---

    /// <summary>"Yellow" | "YellowRed" | "Red" .</summary>
    [JsonPropertyName("card")]
    public string? Card { get; set; }

    [JsonPropertyName("cardDescription")]
    public string? CardDescription { get; set; }

    [JsonIgnore]
    public int SourceIndex { get; set; }

    /// <summary>
    /// Toàn bộ JsonElement gốc của event, dùng để serialize nguyên trạng
    /// vào raw_payload (bao gồm cả shotmapEvent, reactKey, nameStr... không map field riêng).
    /// </summary>
    [JsonIgnore]
    public JsonElement RawJson { get; set; }
}

public class MatchEventPlayerRaw
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}