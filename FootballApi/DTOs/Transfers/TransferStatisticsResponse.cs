namespace FootballApi.DTOs.Transfers;

public sealed record TransferStatisticsResponse
{
    public long TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string Season { get; init; } = string.Empty;
    public decimal TotalFeeToBuy { get; init; }
    public decimal TotalFeeToSell { get; init; }
    public decimal NetSpend { get; init; }
    public int PermanentBuyCount { get; init; }
    public int PermanentSellCount { get; init; }
    public int LoanInCount { get; init; }
    public int LoanOutCount { get; init; }
}