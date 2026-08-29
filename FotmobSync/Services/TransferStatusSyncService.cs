using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using FotmobSync.Services;
using Supabase.Postgrest;

public class TransferStatusSyncService : ITransferStatusSyncService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<TransferSyncService> _logger;

    public TransferStatusSyncService(SupabaseClientFactory factory,
                               ILogger<TransferSyncService> logger
    )
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task SyncAsync(IEnumerable<PlayerTransferStatus> statuses,
                                CancellationToken cancellationToken = default)
    {
        foreach (var resolved in statuses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var existingPlayer = await LoadExistingPlayerAsync(resolved.PlayerId);

                if (existingPlayer == null)
                {
                    _logger.LogWarning(
                           "TransferStatusSync: Player {PlayerId} ({PlayerName}) not found in DB, skipping. " +
                           "Player must be created via Squad sync first.",
                           resolved.PlayerId,
                           resolved.PlayerName);
                    continue;
                }

                if (string.Equals(
                    existingPlayer.TransferStatus,
                    resolved.status,
                    StringComparison.Ordinal
                ))
                {
                    // Unchanged — skip silently, no log needed for the no-op case.
                    continue;
                }

                await _supabase.From<PlayerClean>()
                        .Filter("player_id", Constants.Operator.Equals, resolved.PlayerId)
                        .Set(x => x.TransferStatus, resolved.status)
                        .Update();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
               ex,
               "Failed syncing transfer status for player {PlayerId}",
               resolved.PlayerId);
            }
        }
    }

    private async Task<PlayerClean?> LoadExistingPlayerAsync(long playerId)
    {
        var response = await _supabase.From<PlayerClean>()
                                .Select("player_id, transfer_status")
                                .Filter("player_id", Constants.Operator.Equals, playerId)
                                .Get();
        return response.Models?.FirstOrDefault();
    }
}
