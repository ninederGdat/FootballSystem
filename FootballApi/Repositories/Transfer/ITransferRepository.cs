using FootballApi.DTOs.Players;

namespace FootballApi.Repositories.Transfer;

public interface ITransferRepository
{
    Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct = default);
}