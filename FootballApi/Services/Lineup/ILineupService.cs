using FootballApi.DTOs.Lineups;

public interface ILineupService
{
    Task<LineupResponse> GetLineupByMatchIdAsync(long matchId);
    Task<LineupResponse?> GetLineupByMatchIdOrDefaultAsync(long matchId);
}