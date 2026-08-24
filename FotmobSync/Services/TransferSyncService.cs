using FootballSystem.Shared.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Mappers;
using FotmobSync.Modules;
using FotmobSync.Services;
using Supabase.Postgrest;

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

    public async Task SyncAsync(TeamDataSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var transfersRawList = snapshot.TeamRaw.Transfers.AllTransfers;

        if (transfersRawList == null || transfersRawList.Count == 0)
        {
            _logger.LogInformation("No transfer data found in snapshot, skipping transfer sync.");
            return;
        }

        var cleanList = transfersRawList.ToCleanList();

        // Log the number of skipped records due to unparsable transferDate
        var skippedCount = transfersRawList.Count - cleanList.Count;
        if (skippedCount > 0)
        {
            _logger.LogWarning(
                "{SkippedCount} transfer record(s) skipped due to unparsable transferDate.",
                skippedCount);
        }


        foreach (var incomplete in cleanList.Where(c => c.HasIncompleteTimestamp))
        {
            _logger.LogWarning(
                "Transfer for player {PlayerId} ({PlayerName}) has an incomplete timestamp (fromDate/toDate missing or partial).",
                incomplete.PlayerId, incomplete.PlayerName);
        }

        if (cleanList.Count == 0)
        {
            _logger.LogInformation("No valid transfer records to sync for team {TeamId}.", snapshot.TeamRaw?.Details?.Id);
            return;
        }

        // Convert the list of TransferClean to a list of TransferUpsert for database insertion
        var upsertList = cleanList.ToUpsertList();

        await _supabase
            .From<TransferUpsert>()
            .Upsert(upsertList, new QueryOptions { OnConflict = OnConflictColumns });

        _logger.LogInformation(
            "Synced {Count} transfer record(s) for team {TeamId}.",
            upsertList.Count, snapshot.TeamRaw?.Details?.Id);
    }

}

