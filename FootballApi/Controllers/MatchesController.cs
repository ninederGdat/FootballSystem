using FootballApi.DTOs.Common;
using FootballApi.DTOs.Matches;
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
    [ProducesResponseType(typeof(MatchDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchDetailResponse>> GetMatch(long matchId)
    {
        var result = await _matchService.GetMatchAsync(matchId);
        return Ok(result);
    }

    /// <summary>
    /// Tìm kiếm/lọc danh sách trận đấu theo ngày, đối thủ, trạng thái, CLB, giải đấu (phân trang).
    /// teamId không truyền = trận của cả hai CLB theo dõi; teamId không phải CLB theo dõi → 400.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MatchSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<MatchSummaryResponse>>> SearchMatches(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? season,
        [FromQuery] string? opponent,
         [FromQuery] long? teamId,
       [FromQuery] long? competitionId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 5;

        var query = new MatchSearchQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Season = season,
            Opponent = opponent,
            TeamId = teamId,
            CompetitionId = competitionId,
            Status = status,
            Page = page,
            PageSize = pageSize
        };

        var result = await _matchService.SearchMatchesAsync(query, ct);
        return Ok(result);
    }
}