using FotmobSync.Models.Sync;

namespace FotmobSync.Infrastructure.Resolvers.TransferStatus;

public class TransferStatusResolver : ITransferStatusResolver
{

    public IReadOnlyList<PlayerTransferStatus> Resolve(
        IEnumerable<TransferClean> transfers,
        DateTime currentDate,
         IReadOnlySet<long> trackedTeamIds)
    {
        var results = new List<PlayerTransferStatus>();
        foreach (var group in transfers
          .Where(t => !t.ContractExtension)          // gia hạn hợp đồng không phải di chuyển
          .GroupBy(t => t.PlayerId))
        {
            var latest = group
                .OrderByDescending(t => t.TransferDate)
                .ThenBy(t => t.HasIncompleteTimestamp)
                .ThenByDescending(t => t.Id)
                .First();

            // Bỏ qua nếu không liên quan club tracked nào
            if (!trackedTeamIds.Contains(latest.FromClubId) &&
                !trackedTeamIds.Contains(latest.ToClubId))
                continue;

            results.Add(ResolveOne(latest, currentDate, trackedTeamIds));
        }

        return results;
    }

    private static PlayerTransferStatus ResolveOne(
    TransferClean t, DateTime now, IReadOnlySet<long> tracked)
    {
        var fromTracked = tracked.Contains(t.FromClubId);
        var toTracked = tracked.Contains(t.ToClubId);

        // 1. Cho mượn đang hiệu lực
        if (t.OnLoan && t.PeriodEnd > now)
        {
            return fromTracked
                ? Build(t, parent: t.FromClubId, current: t.ToClubId, "loaned")
                : Build(t, parent: t.ToClubId, current: t.ToClubId, "current"); // mượn từ club ngoài
        }

        // 2. Hết hạn mượn, chưa có sự kiện mới
        //    Quy tắc đã xác nhận: về lại club chủ quản.
        if (t.OnLoan)
        {
            return fromTracked
                ? Build(t, parent: t.FromClubId, current: t.FromClubId, "current")
                : Build(t, parent: t.ToClubId, current: t.ToClubId, "current"); // giữ hành vi cũ
        }

        // 3. Chuyển nhượng vĩnh viễn
        if (toTracked)                       // gia nhập hoặc chuyển nội bộ giữa 2 club tracked
            return Build(t, parent: t.ToClubId, current: t.ToClubId, "current");

        // Rời hẳn: giữ club tracked cuối cùng làm parent 
        return Build(t, parent: t.FromClubId, current: t.ToClubId, "transferred");
    }

    private static PlayerTransferStatus Build(
        TransferClean t, long parent, long current, string status) => new()
        {
            PlayerId = t.PlayerId,
            PlayerName = t.PlayerName,
            FromClubId = t.FromClubId,
            ToClubId = t.ToClubId,
            ParentClubId = parent,
            CurrentClubId = current,
            status = status
        };
}