using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Competition;

public interface ICompetitionRepository
{
    Task<IReadOnlyList<CompetitionClean>> GetAllAsync(CancellationToken ct = default);
}
