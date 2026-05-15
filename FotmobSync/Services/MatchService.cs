using System.Text.Json;
using FotmobSync.Infrastructure;
using FotmobSync.Mappers;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Modules;
using Microsoft.Extensions.Logging;
using Supabase.Postgrest;

namespace FotmobSync.Services;

/// <summary>
/// Đọc fixtures từ payload team (<c>fixtures.allFixtures.fixtures</c>) và upsert vào Supabase.
/// </summary>
public class MatchService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Supabase.Client _supabase;
    private readonly ILogger<MatchService> _logger;

    public MatchService(SupabaseClientFactory factory, ILogger<MatchService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    /// <summary>
    /// Trích xuất <see cref="MatchRaw"/> từ snapshot team (không gọi API).
    /// </summary>
    public List<MatchRaw> ExtractFixtures(TeamDataSnapshot snapshot)
    {
        try
        {
            var array = GetFixturesArray(snapshot.Root);
            if (array.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Team {TeamId}: không tìm thấy fixtures.allFixtures.fixtures (array).", snapshot.TeamId);
                return new List<MatchRaw>();
            }

            var list = new List<MatchRaw>();
            foreach (var el in array.EnumerateArray())
            {
                var match = el.Deserialize<MatchRaw>(JsonOptions);
                if (match != null)
                    list.Add(match);
            }

            _logger.LogInformation("Team {TeamId}: đọc được {Count} fixture từ payload.", snapshot.TeamId, list.Count);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi đọc fixtures cho team {TeamId}", snapshot.TeamId);
            return new List<MatchRaw>();
        }
    }

    /// <summary>
    /// Upsert matches từ snapshot; bỏ qua bản ghi đã tồn tại và dữ liệu không đổi.
    /// </summary>
    public async Task SyncMatchesAsync(TeamDataSnapshot snapshot)
    {
        try
        {
            var rawList = ExtractFixtures(snapshot);
            var cleanList = rawList.ToCleanList();
            if (cleanList.Count == 0)
            {
                _logger.LogInformation("Team {TeamId}: không có match hợp lệ để upsert.", snapshot.TeamId);
                return;
            }

            var existingById = await LoadExistingMatchesAsync(cleanList);
            var now = DateTime.UtcNow;
            var toUpsert = new List<MatchClean>();
            var skipped = 0;

            foreach (var incoming in cleanList)
            {
                if (existingById.TryGetValue(incoming.MatchId, out var existing) &&
                    IsMatchPayloadUnchanged(incoming, existing))
                {
                    skipped++;
                    continue;
                }

                if (existing != null)
                    incoming.CreatedAt = existing.CreatedAt;

                incoming.LastUpdated = now;
                toUpsert.Add(incoming);
            }

            if (toUpsert.Count == 0)
            {
                _logger.LogInformation(
                    "Team {TeamId}: không upsert — {Total} match đã khớp DB (bỏ qua {Skipped}).",
                    snapshot.TeamId, cleanList.Count, skipped);
                return;
            }

            await _supabase
                .From<MatchClean>()
                .Upsert(toUpsert, new() { OnConflict = "match_id" });

            _logger.LogInformation(
                "Team {TeamId}: đã upsert {Upserted} match (bỏ qua không đổi: {Skipped}).",
                snapshot.TeamId, toUpsert.Count, skipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi sync matches cho team {TeamId}", snapshot.TeamId);
        }
    }

    private async Task<Dictionary<long, MatchClean>> LoadExistingMatchesAsync(List<MatchClean> cleanList)
    {
        var ids = cleanList.Select(c => (object)c.MatchId).ToList();
        var response = await _supabase
            .From<MatchClean>()
            .Filter("match_id", Constants.Operator.In, ids)
            .Get();

        return response.Models?.ToDictionary(m => m.MatchId) ?? new Dictionary<long, MatchClean>();
    }

    private static bool IsMatchPayloadUnchanged(MatchClean incoming, MatchClean existing)
    {
        return incoming.HomeTeamId == existing.HomeTeamId
            && incoming.AwayTeamId == existing.AwayTeamId
            && string.Equals(incoming.HomeTeamName, existing.HomeTeamName, StringComparison.Ordinal)
            && string.Equals(incoming.AwayTeamName, existing.AwayTeamName, StringComparison.Ordinal)
            && incoming.HomeScore == existing.HomeScore
            && incoming.AwayScore == existing.AwayScore
            && string.Equals(incoming.TournamentName, existing.TournamentName, StringComparison.Ordinal)
            && incoming.LeagueId == existing.LeagueId
            && incoming.KickoffUtc == existing.KickoffUtc
            && incoming.Started == existing.Started
            && incoming.Finished == existing.Finished
            && incoming.Cancelled == existing.Cancelled;
    }

    private static JsonElement GetFixturesArray(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return default;

        if (!root.TryGetProperty("fixtures", out var fixtures) || fixtures.ValueKind != JsonValueKind.Object)
            return default;

        if (!fixtures.TryGetProperty("allFixtures", out var allFixtures) || allFixtures.ValueKind != JsonValueKind.Object)
            return default;

        if (!allFixtures.TryGetProperty("fixtures", out var arr))
            return default;

        return arr;
    }
}
