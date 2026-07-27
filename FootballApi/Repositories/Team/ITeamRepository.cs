// Repositories/Team/ITeamRepository.cs
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Team;

public interface ITeamRepository
{
    Task<TeamClean?> GetByIdAsync(long teamId, CancellationToken ct);
    Task<List<TeamClean>> GetByIdsAsync(List<long> teamIds, CancellationToken ct = default);
}