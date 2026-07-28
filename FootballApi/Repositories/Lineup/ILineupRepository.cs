using FootballApi.Repositories.Lineup;
using FootballSystem.Shared.Models.Clean;

public interface ILineupRepository
{
    Task<LineupClean?> GetLineupByMatchIdAsync(long matchId, CancellationToken ct = default);
    Task<FormationClean?> GetFormationByIdAsync(long formationId, CancellationToken ct = default);
    Task<List<LineupPlayerClean>> GetLineupPlayersAsync(long lineupId, CancellationToken ct = default);
    Task<List<PlayerClean>> GetPlayersByIdsAsync(List<long> playerIds, CancellationToken ct = default);
    Task<(List<LineupPlayerClean> Items, int TotalCount)> GetLineupPlayersByPlayerIdAsync(
        long playerId, int page, int pageSize, CancellationToken ct = default);

    Task<(List<PlayerAppearanceRecord> Items, int TotalCount)> GetPlayerAppearancesAsync(
        long playerId, int page, int pageSize, CancellationToken ct = default);

    Task<List<LineupClean>> GetByIdsAsync(List<long> lineupIds, CancellationToken ct = default);
}