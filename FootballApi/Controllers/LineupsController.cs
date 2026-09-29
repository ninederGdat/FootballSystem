using FootballApi.DTOs.Lineups;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matches")]
public class LineupsController : ControllerBase
{
    private readonly ILineupService _service;

    public LineupsController(ILineupService service) => _service = service;

    [HttpGet("{matchId}/lineup")]
    public async Task<ActionResult<LineupResponse>> GetLineup(
    long matchId,
    [FromQuery] string? position = null)
    {
        var result = await _service.GetLineupByMatchIdAsync(matchId);

        if (!string.IsNullOrWhiteSpace(position))
        {
            result.Starters = result.Starters
                .Where(p => p.PositionCode == position).ToList();
            result.Substitutes = result.Substitutes
                .Where(p => p.PositionCode == position).ToList();
        }

        return Ok(result);
    }
}