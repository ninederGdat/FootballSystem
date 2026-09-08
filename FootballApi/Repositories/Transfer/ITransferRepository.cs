using FootballApi.DTOs.Players;

namespace FootballApi.Repositories.Transfer;

public interface ITransferRepository
{
    Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct = default);
    Task<(IReadOnlyList<TransferClean> Items, int TotalCount)> SearchAsync(
    string playerName,
    long? playerId,
    long? fromClubId,
    long? toClubId,
    bool? onLoan,
    bool? contractExtension,
    string transferType,
    DateTime? dateFrom,
    DateTime? dateTo,
    int page, int pageSize, CancellationToken ct);
}