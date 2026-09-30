using FootballApi.DTOs.Transfers;

namespace FootballApi.Services.Transfer;

public static class TransferStatisticsMapper
{
    public static TransferStatisticsResponse Map(
        long teamId,
        string teamName,
        string season,
        decimal totalFeeToBuy,
        decimal totalFeeToSell,
        int permanentBuyCount,
        int permanentSellCount,
        int loanInCount,
        int loanOutCount)
    {
        return new TransferStatisticsResponse
        {
            TeamId = teamId,
            TeamName = teamName,
            Season = season,
            TotalFeeToBuy = totalFeeToBuy,
            TotalFeeToSell = totalFeeToSell,
            NetSpend = totalFeeToBuy - totalFeeToSell,
            PermanentBuyCount = permanentBuyCount,
            PermanentSellCount = permanentSellCount,
            LoanInCount = loanInCount,
            LoanOutCount = loanOutCount
        };
    }
}