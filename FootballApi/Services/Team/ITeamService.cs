using FootballApi.DTOs.Teams;

namespace FootballApi.Services.Team;

public interface ITeamService
{
    Task<IReadOnlyList<TeamSummaryResponse>> GetAllAsync(CancellationToken ct = default);
}
