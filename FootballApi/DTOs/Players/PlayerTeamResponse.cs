namespace FootballApi.DTOs.Players;

public record PlayerTeamResponse
{
    public int TeamId { get; init; }
    public string TeamName { get; init; }
}