namespace FootballApi.DTOs.Players;

public class PlayerSummaryResponse
{
    public long PlayerId { get; set; }
    public string Name { get; set; } = default!;
    public string? TeamName { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? Nationality { get; set; }
    public int? ShirtNumber { get; set; }
    public int? Age { get; set; }
    public string? Status { get; set; }
    public decimal? MarketValue { get; set; }
    public DateOnly? ContractUntil { get; set; }
    public string? TransferStatus { get; set; }
}
