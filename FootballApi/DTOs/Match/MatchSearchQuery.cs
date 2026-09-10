namespace FootballApi.DTOs.Matches;

/// <summary>
/// Tham số tìm kiếm/lọc cho GET /api/matches.
/// </summary>
public class MatchSearchQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Season { get; set; }
    public string? Opponent { get; set; }

    /// <summary><c>UPCOMING</c>, <c>ONGOING</c>, hoặc <c>FINISHED</c>.</summary>
    public string? Status { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}