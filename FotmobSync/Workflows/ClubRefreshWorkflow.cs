using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using FotmobSync.Mappers;
using FotmobSync.Modules;
using FotmobSync.Services;

namespace FotmobSync.Workflows;

public class ClubRefreshWorkflow
{
    private readonly FotmobTeamDataModule _teamDataModule;
    private readonly ITeamSyncService _teamSyncService;
    private readonly IMatchSyncService _matchSyncService;
    private readonly ISquadSyncService _squadSyncService;
    private readonly ITransferSyncService _transferSyncService;
    private readonly ITransferStatusResolver _transferStatusReSolver;
    private readonly ITransferStatusSyncService _transferStatusSyncService;
    private readonly ILineupBackfillService _lineupBackfillService;
    private readonly IPlayerSyncService _playerService;
    private readonly ILogger<ClubRefreshWorkflow> _logger;

    public ClubRefreshWorkflow(
        FotmobTeamDataModule teamDataModule,
        ITeamSyncService teamSyncService,
        IMatchSyncService matchSyncService,
        ISquadSyncService squadSyncService,
        ITransferSyncService transferSyncService,
        ITransferStatusResolver transferStatusResolver,
        ITransferStatusSyncService transferStatusSyncService,
        ILineupBackfillService lineupBackfillService,
        IPlayerSyncService playerService,
        ILogger<ClubRefreshWorkflow> logger)
    {
        _teamDataModule = teamDataModule;
        _teamSyncService = teamSyncService;
        _matchSyncService = matchSyncService;
        _squadSyncService = squadSyncService;
        _transferSyncService = transferSyncService;
        _transferStatusReSolver = transferStatusResolver;
        _lineupBackfillService = lineupBackfillService;
        _transferStatusSyncService = transferStatusSyncService;
        _playerService = playerService;

        _logger = logger;
    }

    public async Task ExecuteAsync(
      IReadOnlyCollection<int> teamIds,
      CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting club refresh for teams {TeamIds}", string.Join(",", teamIds));

        // Tập tracked lấy từ cấu hình, không từ snapshot tải được:
        // nếu một team tải lỗi, hướng chuyển nhượng vẫn phải xác định đúng.
        var trackedIds = teamIds.Select(id => (long)id).ToHashSet();

        // Load snapshot cho tất cả team
        var snapshots = new List<TeamDataSnapshot>();
        foreach (var id in teamIds)
        {
            try { snapshots.Add(await _teamDataModule.LoadAsync(id)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot load snapshot for team {TeamId}", id);
            }
        }
        if (snapshots.Count == 0) return;

        await RunPhaseAsync("PHASE 0: Teams", async () =>
        {
            foreach (var s in snapshots)
                await _teamSyncService.SyncAsync(s);
        });

        await RunPhaseAsync("PHASE 1: Squads (distinct players)", () =>
            _squadSyncService.SyncAsync(snapshots, cancellationToken));

        await RunPhaseAsync("PHASE 1b: Enrich stub players", () =>
            _playerService.EnrichAsync(batchSize: 10, cancellationToken));

        await RunPhaseAsync("PHASE 2: Transfers", () =>
            _transferSyncService.SyncAsync(snapshots, cancellationToken));   // overload mới, xem bên dưới

        await RunPhaseAsync("PHASE 3: Resolve transfer status", async () =>
        {
            var merged = TransferMerger.Merge(
        snapshots.SelectMany(s => (s.TeamRaw?.Transfers?.AllTransfers ?? new()).ToCleanList()));
            var statuses = _transferStatusReSolver.Resolve(merged, DateTime.UtcNow, trackedIds);
            await _transferStatusSyncService.SyncAsync(statuses, cancellationToken);
        });

        await RunPhaseAsync("PHASE 4: Matches", async () =>
  {
      foreach (var s in snapshots)
          await _matchSyncService.SyncAsync(s);
  });

        await RunPhaseAsync("PHASE 5: Lineup backfill", async () =>
        {
            foreach (var s in snapshots)
                await _lineupBackfillService.SyncMissingAsync(s.TeamId);
        });

        _logger.LogInformation("Completed club refresh workflow.");
    }

    private async Task RunPhaseAsync(string name, Func<Task> phase)
    {
        _logger.LogInformation("{Phase}", name);
        try { await phase(); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            // Một pha lỗi không làm hỏng các pha sau: nhờ stub, các pha không còn phụ thuộc cứng nhau.
            _logger.LogError(ex, "{Phase} failed", name);
        }
    }

    public static class TransferMerger
    {
        public static readonly TimeSpan Window = TimeSpan.FromHours(48);

        public static List<TransferClean> Merge(IEnumerable<TransferClean> transfers)
        {
            var result = new List<TransferClean>();

            foreach (var group in transfers.GroupBy(
                t => (t.PlayerId, t.FromClubId, t.ToClubId, t.OnLoan)))
            {
                var cluster = new List<TransferClean>();

                foreach (var t in group.OrderBy(x => x.TransferDate))
                {
                    if (cluster.Count > 0 && t.TransferDate - cluster[0].TransferDate > Window)
                    {
                        result.Add(Pick(cluster));
                        cluster.Clear();
                    }
                    cluster.Add(t);
                }

                if (cluster.Count > 0) result.Add(Pick(cluster));
            }

            return result;
        }

        // Ưu tiên bản có timestamp đầy đủ, rồi bản sớm nhất: kết quả ổn định giữa các chu kỳ.
        private static TransferClean Pick(List<TransferClean> cluster) =>
            cluster.OrderBy(t => t.HasIncompleteTimestamp)
                   .ThenBy(t => t.TransferDate)
                   .First();
    }
}