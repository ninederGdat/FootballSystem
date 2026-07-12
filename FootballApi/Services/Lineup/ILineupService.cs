using FootballApi.DTOs.Responses;

public interface ILineupService
{
    Task<LineupResponse> GetLineupByMatchIdAsync(long matchId);
}