namespace FootballApi.DTOs.Players;

public record PlayerTransferStatusResponse
{
    public string CurrentTeam { get; init; }
    public string Status { get; init; }
    public DateOnly PeriodEnd { get; init; }
}