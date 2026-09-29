using FootballApi.DTOs.Common;
using FootballApi.DTOs.Transfers;
using FootballApi.Services.Transfer;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.Controllers;

[ApiController]
[Route("api/transfers")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TransferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<TransferResponse>>> SearchHistoriesTransfers(
    [FromQuery] TransferQuery query,
    CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var (items, totalCount) = await _transferService.SearchTransfersAsync(query, ct);

        return Ok(new PagedResponse<TransferResponse>
        {
            Data = items.Data,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            }
        });
    }
}
