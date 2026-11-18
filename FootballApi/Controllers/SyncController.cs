using FootballApi.DTOs.Sync;
using FootballApi.Services.SyncRun;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api")]
public class SyncController : ControllerBase
{
    private readonly ISyncRunService _syncRunService;

    public SyncController(ISyncRunService syncRunService)
    {
        _syncRunService = syncRunService;
    }

    [HttpGet("sync/runs")]
    [ProducesResponseType(typeof(SyncRunListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SyncRunListResponse>> GetRuns(
        [FromQuery] int? limit, CancellationToken ct = default)
    {
        var runs = await _syncRunService.GetRecentAsync(limit, ct);

        return Ok(new SyncRunListResponse { Data = runs });
    }
}