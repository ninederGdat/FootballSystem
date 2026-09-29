namespace FootballApi.DTOs.Players;

public record PlayerContractResponse
{
    public DateOnly? ContractUntil { get; init; }
    public decimal? MarketValue { get; init; }
}