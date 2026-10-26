using FootballSystem.Shared.Models.Clean;
using FootballSystem.Shared.Players;

namespace FootballApi.Repositories.Player;

public interface IPlayerRepository
{
    Task<PlayerClean?> GetByIdAsync(long playerId, CancellationToken ct);
    Task<List<PlayerClean>> GetByIdsAsync(IEnumerable<long> playerIds, CancellationToken ct);
    Task<(List<PlayerClean> Items, int TotalCount)> SearchAsync(
        string? search,
        long teamId,
        string? positionCode,
        string? nationality,
        string? status,
        PlayerRelation? relation,
        int page, int pageSize, CancellationToken ct);
}