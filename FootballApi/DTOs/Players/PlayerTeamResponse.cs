namespace FootballApi.DTOs.Players;

public record PlayerTeamResponse
{
    public long TeamId { get; init; }
    public string TeamName { get; init; } = default!;
    public string? LogoUrl { get; init; }   // null với CLB không theo dõi
}