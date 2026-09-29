namespace FootballApi.DTOs.Players;

public record PlayerPositionResponse
{
    public string PositionCode { get; init; }
    public string PositionName { get; init; }
}