using FootballApi.DTOs.Competitions;
using FootballApi.Repositories.Competition;

namespace FootballApi.Services.Competition;

public class CompetitionService : ICompetitionService
{
    private readonly ICompetitionRepository _repository;

    public CompetitionService(ICompetitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CompetitionSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var competitions = await _repository.GetAllAsync(ct);

        return competitions
            .Select(competition => new CompetitionSummaryDto
            {
                Id = checked((int)competition.CompetitionId),
                Name = competition.Name
            })
            .ToList();
    }
}
