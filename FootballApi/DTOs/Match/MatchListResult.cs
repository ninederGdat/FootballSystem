namespace FootballApi.DTOs.Matches;

public class MatchListResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<MatchSummaryDTO> Items { get; set; } = [];
}