namespace FootballApi.DTOs.Matches;

/// <summary>
/// Tham số tìm kiếm/lọc cho GET /api/matches.
/// </summary>
public class MatchSearchQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Season { get; set; }

    /// <summary>CLB theo dõi cần xem (team_id HOẶC opponent_team_id). Null = cả hai CLB.</summary>
    public long? TeamId { get; set; }

    public long? CompetitionId { get; set; }

    public string? Opponent { get; set; }

    /// <summary><c>UPCOMING</c>, <c>ONGOING</c>, hoặc <c>FINISHED</c>.</summary>
    public string? Status { get; set; }
    /// <summary>Sắp xếp theo ngày: <c>asc</c> (mặc định, trận sớm nhất trước) hoặc <c>desc</c> (trận mới nhất trước).</summary>
    public string Sort { get; set; } = "asc";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}