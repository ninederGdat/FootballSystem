using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Position;

public interface IPositionRepository
{
    Task<List<PositionClean>> GetAllAsync(CancellationToken ct = default);
    Task<List<PositionClean>> GetByCodesAsync(List<string> positionCodes, CancellationToken ct = default);
}