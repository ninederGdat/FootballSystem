using FootballApi.DTOs.Teams;
using FootballApi.Repositories.Team;

namespace FootballApi.Services.Team;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repository;

    public TeamService(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TeamSummaryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var teams = await _repository.GetAllAsync(ct);

        return teams
            .Select(team => new TeamSummaryResponse
            {
                TeamId = team.TeamId,
                TeamName = team.Name,
                LogoUrl = team.LogoUrl
            })
            .ToList();
    }
}