using FotmobSync.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Mappers;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Modules;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FotmobSync.Services;

public class FotmobEtlService : IFotmobEtlService
{
    private readonly FotmobBrowserClient _browserClient;
    private readonly PositionService _positionService;
    private readonly MatchService _matchService;
    private readonly FotmobTeamDataModule _teamDataModule;
    private readonly Supabase.Client _supabase;
    private readonly ILogger<FotmobEtlService> _logger;

    public FotmobEtlService(
        FotmobBrowserClient browserClient,
        SupabaseClientFactory factory,
        PositionService positionService,
        MatchService matchService,
        FotmobTeamDataModule teamDataModule,
        ILogger<FotmobEtlService> logger)
    {
        _browserClient = browserClient;
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
        _positionService = positionService;
        _matchService = matchService;
        _teamDataModule = teamDataModule;
    }

    /// <summary>
    /// Đồng bộ thông tin đội bóng và toàn bộ squad
    /// </summary>
    public async Task SyncTeamAndSquadAsync(int teamId)
    {
        try
        {
            _logger.LogInformation("Starting full sync for team {TeamId}", teamId);

            var snapshot = await _teamDataModule.LoadAsync(teamId);
            await SyncTeamAsync(snapshot);
            await _matchService.SyncMatchesAsync(snapshot);
            await SyncSquadCoreAsync(snapshot);

            _logger.LogInformation("Completed full sync for team {TeamId}", teamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SyncTeamAndSquadAsync for team {TeamId}", teamId);
        }
    }

    private async Task SyncTeamAsync(TeamDataSnapshot snapshot)
    {
        try
        {
            _logger.LogInformation("Syncing team {TeamId}", snapshot.TeamId);

            var teamRaw = snapshot.TeamRaw;
            if (teamRaw == null)
            {
                _logger.LogWarning("Team {TeamId}: deserialize TeamRaw null, bỏ qua upsert team.", snapshot.TeamId);
                return;
            }

            var teamClean = teamRaw.ToClean();

            await _supabase.From<TeamClean>().Upsert(teamClean);

            _logger.LogInformation("✅ Team '{TeamName}' synced successfully", teamClean.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing team {TeamId}", snapshot.TeamId);
        }
    }

    /// <summary>
    /// Đồng bộ toàn bộ squad của đội (một lần gọi team API).
    /// </summary>
    public async Task SyncSquadAsync(int teamId)
    {
        try
        {
            var snapshot = await _teamDataModule.LoadAsync(teamId);
            await SyncSquadCoreAsync(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing squad for team {TeamId}", teamId);
        }
    }

    private async Task SyncSquadCoreAsync(TeamDataSnapshot snapshot)
    {
        try
        {
            var players = ExtractSquadFromSnapshot(snapshot);

            _logger.LogInformation("Found {Count} players in squad. Starting detailed sync...", players.Count);

            foreach (var player in players)
            {
                await SyncPlayerAsync((int)player.PlayerId, player.TeamId);
                await Task.Delay(6000); // Tăng delay vì Playwright chậm hơn
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing squad for team {TeamId}", snapshot.TeamId);
        }
    }

    public async Task<List<PlayerClean>> ExtractSquadAsync(int teamId)
    {
        try
        {
            var snapshot = await _teamDataModule.LoadAsync(teamId);
            return ExtractSquadFromSnapshot(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting squad for team {TeamId}", teamId);
            return new List<PlayerClean>();
        }
    }

    private List<PlayerClean> ExtractSquadFromSnapshot(TeamDataSnapshot snapshot)
    {
        var rawTeam = snapshot.TeamRaw;
        if (rawTeam?.Squad == null)
            return new List<PlayerClean>();

        var players = new List<PlayerClean>();

        foreach (var group in rawTeam.Squad.Groups)
        {
            foreach (var p in group.Members)
            {
                var playerClean = new PlayerClean
                {
                    PlayerId = p.Id,
                    TeamId = snapshot.TeamId,
                    Name = p.Name ?? string.Empty,
                    ShirtNumber = ParseShirtNumber(p.ShirtNumber),
                    Nationality = p.CountryCode,
                };

                players.Add(playerClean);
            }
        }

        _logger.LogInformation("Extracted {Count} players from team {TeamId}", players.Count, snapshot.TeamId);
        return players;
    }

    /// <summary>
    /// Đồng bộ chi tiết cầu thủ - Đã tích hợp PositionService
    /// </summary>
    public async Task SyncPlayerAsync(int playerId, long teamId)
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

            if (playerRaw.PositionDescription != null)
            {
                await _positionService.UpsertPositionAsync(playerRaw.PositionDescription);
            }

            var playerClean = playerRaw.ToClean(teamId);

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

    private int? ParseShirtNumber(JsonElement? element)
    {
        if (!element.HasValue) return null;
        var el = element.Value;

        if (el.ValueKind == JsonValueKind.Number)
            return el.GetInt32();

        if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out int num))
            return num;

        return null;
    }
}
