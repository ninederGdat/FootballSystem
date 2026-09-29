using FootballApi.DTOs.Players;
using FootballApi.Services.Player;
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
    [ProducesResponseType(typeof(PlayerAppearancesResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlayerAppearancesResult>> GetPlayerAppearances(
        int playerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var (items, totalCount) = await _playerService.GetPlayerAppearancesAsync(playerId, page, pageSize, ct);

        return Ok(new PlayerAppearancesResult
        {
            PlayerId = playerId,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Appearances = items.ToList()
        });
    }

    /// <summary>Tìm kiếm cầu thủ theo tên, đội, vị trí, hoặc quốc tịch (phân trang)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PlayerSearchResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlayerSearchResult>> SearchPlayers(
        [FromQuery] string? search,
        [FromQuery] int? teamId,
        [FromQuery] string? positionCode,
        [FromQuery] string? nationality,
        [FromQuery] string? transferStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var query = new PlayerSearchQuery
        {
            Search = search,
            TeamId = teamId,
            PositionCode = positionCode,
            Nationality = nationality,
            TransferStatus = transferStatus,
            Page = page,
            PageSize = pageSize
        };

        var (items, totalCount) = await _playerService.SearchPlayersAsync(query, ct);

        return Ok(new PlayerSearchResult
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.ToList()
        });
    }
}