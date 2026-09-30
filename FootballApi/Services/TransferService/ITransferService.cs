using FootballApi.DTOs.Common;
using FootballApi.DTOs.Players;
using FootballApi.DTOs.Transfers;

namespace FootballApi.Services.Transfer;

public interface ITransferService
{
    Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct);
    Task<PlayerTransferStatusResponse> GetPlayerTransferStatusAsync(long playerId, long teamId, CancellationToken ct);
    Task<(PagedResponse<TransferResponse> Items, int TotalCount)> SearchTransfersAsync(
    TransferQuery query, CancellationToken ct);
    Task<TransferStatisticsResponse> GetTransferStatisticsAsync(
        TransferStatisticsQuery query, CancellationToken ct);


}