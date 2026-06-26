using FotmobSync.Infrastructure.External;
using FotmobSync.Services;
using FotmobSync.Mappers;
using FotmobSync.Models.Clean;
using Supabase.Postgrest;
using FotmobSync.Infrastructure;
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


    public async Task SyncAsync(
        int playerId,
        long teamId)
    {
        try
        {
            _logger.LogInformation("🔄 Syncing detailed info for player {PlayerId}", playerId);

            var playerRaw = await _browserClient.GetPlayerDetailAsync(playerId);

            if (playerRaw == null)
            {
                _logger.LogWarning("⚠️ Cannot get data for player {PlayerId}", playerId);
                return;
            }

            var playerClean = playerRaw.ToClean(teamId);

            var existingPlayer = await LoadExistingPlayerAsync(playerClean.PlayerId);
            if (existingPlayer != null && IsPlayerPayloadUnchanged(playerClean, existingPlayer))
            {
                _logger.LogInformation(
                    "Player {PlayerId} '{PlayerName}': không đổi, bỏ qua upsert.",
                    playerId, playerClean.Name);
                return;
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

            _logger.LogInformation("✅ Player '{PlayerName}' (ID: {PlayerId}) synced successfully",
                playerClean.Name, playerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error syncing player {PlayerId}", playerId);
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
                StringComparison.Ordinal);
    }
}