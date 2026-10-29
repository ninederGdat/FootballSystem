using FootballApi.DTOs.Teams;
using FootballApi.Services.Team;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>Danh sách CLB được theo dõi (tên, logo) cho bộ chọn CLB ở frontend</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TeamListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TeamListResponse>> GetTeams(CancellationToken ct = default)
    {
        var teams = await _teamService.GetAllAsync(ct);

        return Ok(new TeamListResponse { Data = teams });
    }
}