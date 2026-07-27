using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.PositionRole;

public interface IPositionRoleRepository
{
    Task<List<PositionRoleClean>> GetAllAsync(CancellationToken ct = default);
    Task<List<PositionRoleClean>> GetByIdsAsync(List<int> roleIds, CancellationToken ct = default);
}