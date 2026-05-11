using FotmobSync.Clients;
using FotmobSync.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Mappers;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FotmobSync.Services;

public class FotmobEtlService : IFotmobEtlService
{
    private readonly FotmobClient _httpClient;           // Giữ lại để dùng cho Team
    private readonly FotmobBrowserClient _browserClient; // Dùng cho Player Detail
    private readonly PositionService _positionService;        // ← Thêm
    private readonly Supabase.Client _supabase;
    private readonly ILogger<FotmobEtlService> _logger;

    public FotmobEtlService(
        FotmobClient httpClient,
        FotmobBrowserClient browserClient,        // ← Thêm vào đây
        SupabaseClientFactory factory,
        PositionService positionService,
        ILogger<FotmobEtlService> logger)
    {
        _httpClient = httpClient;
        _browserClient = browserClient;
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
        _positionService = positionService;
    }

    /// <summary>
    /// Đồng bộ thông tin đội bóng và toàn bộ squad
    /// </summary>
    public async Task SyncTeamAndSquadAsync(int teamId)
    {
        try
        {
            _logger.LogInformation("Starting full sync for team {TeamId}", teamId);

            await SyncTeamAsync(teamId);
            await SyncSquadAsync(teamId);

            _logger.LogInformation("Completed full sync for team {TeamId}", teamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SyncTeamAndSquadAsync for team {TeamId}", teamId);
        }
    }

    private async Task SyncTeamAsync(int teamId)
    {
        try
        {
            _logger.LogInformation("Syncing team {TeamId}", teamId);

            using var doc = await _httpClient.GetTeamDataAsync(teamId);
            var teamRaw = doc.RootElement.Deserialize<TeamRaw>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (teamRaw == null) return;

            var teamClean = teamRaw.ToClean();

            await _supabase.From<TeamClean>().Upsert(teamClean);

            _logger.LogInformation("✅ Team '{TeamName}' synced successfully", teamClean.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing team {TeamId}", teamId);
        }
    }

    /// <summary>
    /// Đồng bộ toàn bộ squad của đội
    /// </summary>
    public async Task SyncSquadAsync(int teamId)
    {
        try
        {
            var players = await ExtractSquadAsync(teamId);

            _logger.LogInformation("Found {Count} players in squad. Starting detailed sync...", players.Count);

            foreach (var player in players)
            {
                await SyncPlayerAsync((int)player.PlayerId, player.TeamId);
                await Task.Delay(6000); // Tăng delay vì Playwright chậm hơn
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing squad for team {TeamId}", teamId);
        }
    }

    public async Task<List<PlayerClean>> ExtractSquadAsync(int teamId)
    {
        try
        {
            using var doc = await _httpClient.GetTeamDataAsync(teamId);
            var rawTeam = doc.RootElement.Deserialize<TeamRaw>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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
                        TeamId = teamId,
                        Name = p.Name ?? string.Empty,
                        ShirtNumber = ParseShirtNumber(p.ShirtNumber),
                        Nationality = p.CountryCode,
                    };

                    players.Add(playerClean);
                }
            }

            _logger.LogInformation("Extracted {Count} players from team {TeamId}", players.Count, teamId);
            return players;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting squad for team {TeamId}", teamId);
            return new List<PlayerClean>();
        }
    }

    /// <summary>
    /// Đồng bộ chi tiết cầu thủ - Sử dụng Playwright để bypass Turnstile
    /// </summary>
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

            // === UPSERT POSITION TRƯỚC (Giải quyết lỗi Foreign Key) ===
            if (playerRaw.PositionDescription != null)
            {
                await _positionService.UpsertPositionAsync(playerRaw.PositionDescription);
            }

            // === Transform và Upsert Player ===
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