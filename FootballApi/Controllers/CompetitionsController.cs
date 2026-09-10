using FootballApi.DTOs.Competitions;
using FootballApi.Services.Competition;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api")]
public class CompetitionsController : ControllerBase
{
    private readonly ICompetitionService _competitionService;

    public CompetitionsController(ICompetitionService competitionService)
    {
        _competitionService = competitionService;
    }

    [HttpGet("competitions")]
    [ProducesResponseType(typeof(CompetitionListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompetitionListResponse>> GetCompetitions(CancellationToken ct = default)
    {
        var competitions = await _competitionService.GetAllAsync(ct);

        return Ok(new CompetitionListResponse
        {
            Data = competitions
        });
    }
}
