namespace FootballApi.DTOs.Players;

public record PlayerStatusResponse
{
    public string Status { get; init; }
    public string? InjuryDescription { get; init; }
}