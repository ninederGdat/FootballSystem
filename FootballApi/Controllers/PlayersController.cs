using FootballApi.DTOs.Common;
using FootballApi.DTOs.Players;
using FootballApi.Services.Player;
using FootballSystem.Shared.Players;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api/players")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    /// <summary>Lấy hồ sơ chi tiết một cầu thủ</summary>
    [HttpGet("{playerId:long}")]
    [ProducesResponseType(typeof(PlayerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerProfileResponse>> GetPlayer(int playerId, CancellationToken ct)
    {
        var result = await _playerService.GetPlayerProfileAsync(playerId, ct);
        return Ok(result);
    }

    /// <summary>Lấy lịch sử ra sân của một cầu thủ (phân trang)</summary>
    [HttpGet("{playerId:long}/appearances")]
    [ProducesResponseType(typeof(PagedResponse<PlayerAppearanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PlayerAppearanceResponse>>> GetPlayerAppearances(
        int playerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await _playerService.GetPlayerAppearancesAsync(playerId, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Tìm kiếm cầu thủ theo tên, đội, vị trí, quốc tịch, hoặc trạng thái (phân trang)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PlayerSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PlayerSummaryResponse>>> SearchPlayers(
        [FromQuery] string? search,
        [FromQuery] long? teamId,
        [FromQuery] string? positionCode,
        [FromQuery] string? nationality,
        [FromQuery] string? status,
        [FromQuery] string? transferStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {

        if (teamId is null)
            return BadRequest(new { error = "teamId is required." });

        PlayerRelation? relation = null;
        if (!string.IsNullOrWhiteSpace(transferStatus))
        {
            if (!PlayerRelationExtensions.TryParse(transferStatus, out var parsed))
                return BadRequest(new
                {
                    error = $"Invalid transferStatus '{transferStatus}'. " +
                            "Allowed: current, loaned_in, loaned_out, transferred."
                });
            relation = parsed;
        }

        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var query = new PlayerSearchQuery
        {
            Search = search,
            TeamId = teamId ?? 0,
            PositionCode = positionCode,
            Nationality = nationality,
            Status = status,
            TransferStatus = relation ?? PlayerRelation.Current,
            Page = page,
            PageSize = pageSize
        };

        var result = await _playerService.SearchPlayersAsync(query, ct);
        return Ok(result);
    }
}