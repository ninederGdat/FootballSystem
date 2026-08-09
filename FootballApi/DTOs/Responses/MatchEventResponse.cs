namespace FootballApi.DTOs.Responses;

public class MatchEventResponse
{
    public long Id { get; set; }
    public int Minute { get; set; }
    public int? StoppageTime { get; set; }
    public string EventType { get; set; } = default!;
    public string? DescriptionKey { get; set; }

    public long? TeamId { get; set; }

    public long? PlayerId { get; set; }
    /// <summary>Tên từ PlayerRepository nếu PlayerId có; fallback từ raw_payload nếu null (đội khách/event ẩn danh).</summary>
    public string? PlayerName { get; set; }

    public long? AssistPlayerId { get; set; }
    public string? AssistPlayerName { get; set; }
}