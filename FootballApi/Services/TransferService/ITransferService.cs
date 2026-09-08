using FootballApi.DTOs.Players;
using FootballApi.DTOs.Responses;
using FootballApi.DTOs.Transfers;

namespace FootballApi.Services.Transfer;

public interface ITransferService
{
    Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct);
    Task<PlayerTransferStatusDTO> GetPlayerTransferStatusAsync(long playerId, long teamId, CancellationToken ct);
    Task<(PagedResponse<TransferResponse> Items, int TotalCount)> SearchTransfersAsync(
    TransferQuery query, CancellationToken ct);


}