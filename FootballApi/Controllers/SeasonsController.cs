using FootballApi.DTOs.Seasons;
using FootballApi.Services.Season;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api")]
public class SeasonsController : ControllerBase
{
    private readonly ISeasonService _seasonService;

    public SeasonsController(ISeasonService seasonService)
    {
        _seasonService = seasonService;
    }

    [HttpGet("seasons")]
    [ProducesResponseType(typeof(SeasonListResponse), StatusCodes.Status200OK)]
    public ActionResult<SeasonListResponse> GetSeasons()
    {
        var seasons = _seasonService.GetAvailableSeasons();

        return Ok(new SeasonListResponse
        {
            Data = seasons
                .Select(season => new SeasonSummaryResponse
                {
                    Code = season.Code,
                    Name = season.Name
                })
                .ToList()
        });
    }
}
