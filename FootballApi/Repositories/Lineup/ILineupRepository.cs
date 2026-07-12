using FootballSystem.Shared.Models.Clean;

public interface ILineupRepository
{
    Task<LineupClean?> GetLineupByMatchIdAsync(long matchId);
    Task<FormationClean?> GetFormationByIdAsync(long formationId);
    Task<List<LineupPlayerClean>> GetLineupPlayersAsync(long lineupId);
    Task<List<PlayerClean>> GetPlayersByIdsAsync(List<long> playerIds);
     Task<List<PositionClean>> GetPositionsByCodesAsync(List<string> positionCodes);
    Task<List<PositionRoleClean>> GetPositionRolesByIdsAsync(List<int> roleIds);
}