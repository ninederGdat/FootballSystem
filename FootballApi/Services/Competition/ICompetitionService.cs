using FootballApi.DTOs.Competitions;

namespace FootballApi.Services.Competition;

public interface ICompetitionService
{
    Task<IReadOnlyList<CompetitionSummaryResponse>> GetAllAsync(CancellationToken ct = default);
}
