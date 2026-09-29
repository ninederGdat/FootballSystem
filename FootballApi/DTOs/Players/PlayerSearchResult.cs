namespace FootballApi.DTOs.Players;

public class PlayerSearchResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<PlayerSummaryResponse> Items { get; set; } = [];
}