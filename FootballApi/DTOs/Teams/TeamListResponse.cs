namespace FootballApi.DTOs.Teams;

public class TeamListResponse
{
    public IReadOnlyList<TeamSummaryResponse> Data { get; set; } = [];
}