using FotmobSync.Models.Sync;

namespace FotmobSync.Infrastructure.Resolvers.TransferStatus;

public class TransferStatusResolver : ITransferStatusResolver
{

    public IReadOnlyList<PlayerTransferStatus> Resolve(
        IEnumerable<TransferClean> transfers,
        DateTime currentDate,
        long teamId)
    {
        var transferHistory = transfers
            .GroupBy(t => t.PlayerId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(t => t.TransferDate)
                    .ToList());

        var results = new List<PlayerTransferStatus>();

        foreach (var (playerId, playerTransfers) in transferHistory)
        {
            var latestTransfer = playerTransfers[0];

            var status = ResolveStatus(
                latestTransfer,
                currentDate,
                teamId);

            results.Add(new PlayerTransferStatus
            {
                PlayerId = latestTransfer.PlayerId,
                PlayerName = latestTransfer.PlayerName,
                FromClubId = latestTransfer.FromClubId,
                ToClubId = latestTransfer.ToClubId,
                status = status
            });
        }

        return results;
    }

    /// <summary>
    /// Resolves the transfer status based on the single latest transfer event
    /// for a player.
    /// </summary>
    private static string ResolveStatus(
        TransferClean latestTransfer,
        DateTime currentDate,
        long teamId)
    {
        /*
         * Case 1:
         * Latest event is the player returning to Chelsea.
         *
         * Example:
         * Everton -> Chelsea
         */
        if (latestTransfer.ToClubId == teamId)
        {
            return "current";
        }

        /*
         * Case 2:
         * Latest event is the player leaving Chelsea.
         *
         * Example:
         * Chelsea -> Everton
         */
        if (latestTransfer.FromClubId == teamId)
        {
            /*
             * Loan
             */
            if (latestTransfer.OnLoan)
            {
                /*
                 * Loan is still active.
                 */
                if (latestTransfer.PeriodEnd > currentDate)
                {
                    return "loaned";
                }

                /*
                 * Loan has expired and no newer event (e.g. an explicit
                 * "return from loan" transfer) exists yet in the history.
                 *
                 * Business rule (confirmed): an expired loan with no newer
                 * event implies the player has rejoined Chelsea.
                 */
                return "current";
            }

            /*
             * Permanent transfer.
             */
            if (latestTransfer.TransferType == "contract")
            {
                return "transferred";
            }
        }

        /*
         * Fallback:
         * Latest event doesn't cleanly match a known pattern
         * (e.g. FromClubId/ToClubId neither is teamId — shouldn't happen
         * since TransferSyncService only persists transfers touching
         * teamId, but kept defensive).
         */
        return "current";
    }
}