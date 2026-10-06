using FotmobSync.Infrastructure.External;
using FotmobSync.Services;
using FotmobSync.Mappers;
using Supabase.Postgrest;
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
public class PlayerSyncService
    : IPlayerSyncService
{
    private readonly FotmobBrowserClient _browserClient;
    private readonly PositionService _positionService;
    private readonly Supabase.Client _supabase;
    private readonly ILogger<PlayerSyncService> _logger;

    public PlayerSyncService(FotmobBrowserClient browserClient,
                             PositionService positionService,
                             SupabaseClientFactory factory,
                             ILogger<PlayerSyncService> logger)
    {
        _browserClient = browserClient;
        _positionService = positionService;
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }


    public async Task<bool> SyncAsync(
    long playerId,
    long teamId,
     bool useProfileTeam = false,
    CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Syncing detailed info for player {PlayerId}", playerId);

            var playerRaw = await _browserClient.GetPlayerDetailAsync(playerId);

            if (playerRaw == null)
            {
                _logger.LogWarning("Cannot get data for player {PlayerId}", playerId);
                return false;
            }


            var profileTeamId = playerRaw.PrimaryTeam?.TeamId;   // đối chiếu tên/kiểu property trong PlayerRaw
            var resolvedTeamId = useProfileTeam && profileTeamId.HasValue
                ? profileTeamId.Value
                : teamId;

            var playerClean = playerRaw.ToClean(resolvedTeamId);
            playerClean.IsStub = false;

            var existingPlayer = await LoadExistingPlayerAsync(playerClean.PlayerId);

            // If the player already exists, update it instead of inserting it.
            playerClean.TransferStatus = existingPlayer?.TransferStatus ?? playerClean.TransferStatus;

            if (existingPlayer is { IsStub: false })
            {
                playerClean.TeamId = existingPlayer.TeamId;
                playerClean.CurrentTeamId = existingPlayer.CurrentTeamId; // tránh bị upsert ghi null
            }

            if (existingPlayer is { IsStub: false } && IsPlayerPayloadUnchanged(playerClean, existingPlayer))
            {
                _logger.LogInformation(
                    "Player {PlayerId} '{PlayerName}': không đổi, bỏ qua upsert.",
                    playerId, playerClean.Name);
                return true;
            }

            if (playerRaw.PositionDescription != null)
            {
                await _positionService.UpsertPositionAsync(playerRaw.PositionDescription);
            }

            if (existingPlayer != null)
                playerClean.CreatedAt = existingPlayer.CreatedAt;

            playerClean.LastUpdated = DateTime.UtcNow;

            await _supabase
                .From<PlayerClean>()
                .Upsert(playerClean, new() { OnConflict = "player_id" });

            _logger.LogInformation("Player '{PlayerName}' (ID: {PlayerId}) synced successfully",
                playerClean.Name, playerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing player {PlayerId}", playerId);
            return false;
        }
    }

    private async Task<PlayerClean?> LoadExistingPlayerAsync(long playerId)
    {
        var response = await _supabase
            .From<PlayerClean>()
            .Filter("player_id", Constants.Operator.Equals, playerId)
            .Get();

        return response.Models?.FirstOrDefault();
    }



    public async Task EnrichAsync(int batchSize = 10, CancellationToken cancellationToken = default)
    {
        var stubs = await _supabase.From<PlayerClean>()
            .Filter("is_stub", Constants.Operator.Equals, "true")
            .Order("last_updated", Constants.Ordering.Ascending)
            .Limit(batchSize)
            .Get();

        foreach (var stub in stubs.Models)
        {
            var ok = await SyncAsync(stub.PlayerId, stub.TeamId, useProfileTeam: true);
            if (!ok)   // đẩy xuống cuối hàng đợi để không kẹt ở một player lỗi
                await _supabase.From<PlayerClean>()
                    .Where(x => x.PlayerId == stub.PlayerId)
                    .Set(x => x.LastUpdated, DateTime.UtcNow)
                    .Update();

            await Task.Delay(TimeSpan.FromSeconds(6));
        }
    }

    private static bool IsPlayerPayloadUnchanged(PlayerClean incoming, PlayerClean existing)
    {
        return incoming.PlayerId == existing.PlayerId
            && incoming.TeamId == existing.TeamId
            && string.Equals(incoming.Name, existing.Name, StringComparison.Ordinal)
            && incoming.ShirtNumber == existing.ShirtNumber
            && incoming.DateOfBirth == existing.DateOfBirth
            && string.Equals(incoming.Nationality, existing.Nationality, StringComparison.Ordinal)
            && incoming.ContractUntil == existing.ContractUntil
            && incoming.MarketValue == existing.MarketValue
            && string.Equals(incoming.Status, existing.Status, StringComparison.Ordinal)
            && string.Equals(incoming.InjuryDescription, existing.InjuryDescription, StringComparison.Ordinal)
            && string.Equals(
                incoming.PreferredPositionCode,
                existing.PreferredPositionCode,
                StringComparison.Ordinal)
            && string.Equals(
                incoming.TransferStatus,
                existing.TransferStatus,
                StringComparison.Ordinal)
                ;

    }
}