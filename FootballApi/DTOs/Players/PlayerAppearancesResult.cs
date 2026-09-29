namespace FootballApi.DTOs.Players;

public class PlayerAppearancesResult
{
    public long PlayerId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<PlayerAppearanceResponse> Appearances { get; set; } = [];
}