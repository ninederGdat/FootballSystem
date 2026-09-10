using FootballApi.DTOs.Competitions;

namespace FootballApi.Services.Competition;

public interface ICompetitionService
{
    Task<IReadOnlyList<CompetitionSummaryDto>> GetAllAsync(CancellationToken ct = default);
}
