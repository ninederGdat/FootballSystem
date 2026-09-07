using FootballApi.DTOs.Players;

namespace FootballApi.Services.Transfer;

public interface ITransferService
{
    Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct);
    Task<PlayerTransferStatusDTO> GetPlayerTransferStatusAsync(long playerId, long teamId, CancellationToken ct);
}