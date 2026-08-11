using FootballApi.DTOs.Matches;
using FootballApi.DTOs.Responses;
using FootballApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    /// <summary>Lấy thông tin chi tiết một trận đấu</summary>
    [HttpGet("{matchId:long}")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponse>> GetMatch(long matchId)
    {
        var result = await _matchService.GetMatchAsync(matchId);
        return Ok(result);
    }

    /// <summary>Tìm kiếm/lọc danh sách trận đấu theo ngày, đối thủ, trạng thái (phân trang)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(MatchListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<MatchListResult>> SearchMatches(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? opponent,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;
 
        var query = new MatchSearchQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Opponent = opponent,
            Status = status,
            Page = page,
            PageSize = pageSize
        };
 
        var (items, totalCount) = await _matchService.SearchMatchesAsync(query, ct);
 
        return Ok(new MatchListResult
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.ToList()
        });
    }
}