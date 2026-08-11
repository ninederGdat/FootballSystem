namespace FootballApi.DTOs.Matches;

/// <summary>
/// Thông tin tóm tắt một trận đấu, dùng cho danh sách (không kèm events/lineup —
/// những phần đó chỉ có trong MatchResponse chi tiết theo matchId).
/// </summary>
public class MatchSummaryDTO
{
    public long MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public string? HomeOrAway { get; set; }
    public string? CompetitionName { get; set; }
    public int? ScoreHome { get; set; }
    public int? ScoreAway { get; set; }
    public string Status { get; set; } = "UPCOMING";
}