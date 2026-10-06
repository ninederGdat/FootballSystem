using FootballSystem.Shared.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Mappers;
using FotmobSync.Modules;
using FotmobSync.Services;
using Supabase.Postgrest;
using static FotmobSync.Workflows.ClubRefreshWorkflow;

public class TransferSyncService
    : ITransferSyncService
{
    // Natural key: no surrogate unique constraint besides (player_id, transfer_date, to_club_id).
    // Free agents use ToClubId = 2 (never null), so this key is always well-defined.
    private const string OnConflictColumns = "player_id,transfer_date,to_club_id";

    private readonly Supabase.Client _supabase;
    private readonly ILogger<TransferSyncService> _logger;

    public TransferSyncService(SupabaseClientFactory factory,
                               ILogger<TransferSyncService> logger)
    {

        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public Task SyncAsync(TeamDataSnapshot snapshot, CancellationToken cancellationToken = default)
    => SyncAsync(new[] { snapshot }, cancellationToken);

    public async Task SyncAsync(
        IReadOnlyCollection<TeamDataSnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        var all = new List<TransferClean>();

        foreach (var snapshot in snapshots)
        {
            var raw = snapshot.TeamRaw?.Transfers?.AllTransfers;
            if (raw == null || raw.Count == 0)
            {
                _logger.LogInformation("Team {TeamId}: no transfer data.", snapshot.TeamId);
                continue;
            }

            var clean = raw.ToCleanList();
            var skipped = raw.Count - clean.Count;
            if (skipped > 0)
                _logger.LogWarning(
                    "Team {TeamId}: {Skipped} transfer record(s) skipped due to unparsable transferDate.",
                    snapshot.TeamId, skipped);

            all.AddRange(clean);
        }

        if (all.Count == 0) return;

        var merged = TransferMerger.Merge(all);
        await AdoptExistingKeysAsync(merged);

        // Chặn trùng khóa conflict ngay trong một batch
        // (Postgres sẽ báo lỗi "cannot affect row a second time").
        var unique = merged
            .GroupBy(t => (t.PlayerId, t.TransferDate, t.ToClubId))
            .Select(g => g.First())
            .ToList();

        foreach (var incomplete in unique.Where(t => t.HasIncompleteTimestamp))
        {
            _logger.LogWarning(
                "Transfer for player {PlayerId} ({PlayerName}) has an incomplete timestamp.",
                incomplete.PlayerId, incomplete.PlayerName);
        }

        await _supabase
            .From<TransferUpsert>()
            .Upsert(unique.ToUpsertList(), new QueryOptions { OnConflict = OnConflictColumns });

        _logger.LogInformation(
            "Synced {Upserted} transfer(s) (raw {Raw}, after merge {Merged}).",
            unique.Count, all.Count, merged.Count);
    }

    private async Task AdoptExistingKeysAsync(List<TransferClean> merged)
    {
        var existing = new List<TransferClean>();

        foreach (var chunk in merged.Select(t => t.PlayerId).Distinct().Chunk(100))
        {
            var res = await _supabase.From<TransferClean>()
                .Filter("player_id", Constants.Operator.In,
                        chunk.Select(x => (object)x).ToList())
                .Get();
            existing.AddRange(res.Models);
        }

        var byKey = existing
            .Where(e => !e.IsSystemGenerated)
            .ToLookup(e => (e.PlayerId, e.FromClubId, e.ToClubId, e.OnLoan));

        foreach (var t in merged)
        {
            var match = byKey[(t.PlayerId, t.FromClubId, t.ToClubId, t.OnLoan)]
                .Where(e => (e.TransferDate - t.TransferDate).Duration() <= TransferMerger.Window)
                .OrderBy(e => e.Id)
                .FirstOrDefault();

            if (match != null)
                t.TransferDate = match.TransferDate;   // giữ khóa cũ, tránh tạo dòng mới
        }
    }



}

