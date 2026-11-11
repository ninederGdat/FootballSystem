namespace FootballApi.DTOs.Matches;

/// <summary>
/// Thông tin tóm tắt một trận đấu, dùng cho danh sách (không kèm events/lineup —
/// những phần đó chỉ có trong MatchDetailResponse chi tiết theo matchId).
/// </summary>
public class MatchSummaryResponse
{
    public long MatchId { get; set; }

    /// <summary>CLB mà hàng này mô tả (OpponentName/HomeOrAway tính theo CLB này).</summary>
    public long? TeamId { get; set; }
    public long? OpponentTeamId { get; set; }
    public DateTime MatchDate { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public string? HomeOrAway { get; set; }
    public string? CompetitionName { get; set; }
    /// <summary>Tỉ số hết giờ (sau hiệp phụ nếu có), không gồm luân lưu. Luôn theo thứ tự chủ nhà – khách.</summary>
    public int? ScoreHome { get; set; }
    public int? ScoreAway { get; set; }

    /// <summary>Điểm luân lưu (chủ nhà – khách); null nếu trận không có luân lưu.</summary>
    public int? PenaltyHome { get; set; }
    public int? PenaltyAway { get; set; }
    public string Status { get; set; } = "UPCOMING";
}