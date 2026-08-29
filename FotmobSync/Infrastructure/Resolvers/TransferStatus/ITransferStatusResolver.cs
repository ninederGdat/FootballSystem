
using FotmobSync.Models.Sync;

namespace FotmobSync.Infrastructure.Resolvers.TransferStatus;

public interface ITransferStatusResolver
{
    IReadOnlyList<PlayerTransferStatus> Resolve(
        IEnumerable<TransferClean> transfers,
        DateTime currentDate,
        long teamId
        );
}

