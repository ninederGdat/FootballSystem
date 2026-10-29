namespace FootballApi.DTOs.Teams;

/// <summary>CLB được theo dõi (có trong bảng teams). Cùng shape với PlayerTeamResponse.</summary>
public class TeamSummaryResponse
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}