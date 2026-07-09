using System.Globalization;
using System.Text.Json;
using FootballSystem.Shared.Infrastructure;
using FotmobSync.Mappers;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Modules;
using FotmobSync.Workflows;
using Microsoft.Extensions.Logging;
using FootballSystem.Shared.Models.Clean;
using Supabase.Postgrest;

namespace FotmobSync.Services;

/// <summary>
/// Đọc fixtures từ payload team và upsert vào <c>matches</c> / <c>competitions</c>.
/// </summary>
public class MatchService : IMatchSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Supabase.Client _supabase;
    private readonly ILogger<MatchService> _logger;
    private readonly ILineupBackfillService _lineupBackfillService;

    public MatchService(SupabaseClientFactory factory,
                        ILogger<MatchService> logger,
                        ILineupBackfillService  lineupBackfillService)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
         _lineupBackfillService = lineupBackfillService;
    }

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

    public async Task SyncAsync(TeamDataSnapshot snapshot)
    {
        await SyncMatchesAsync(snapshot);
    }

    public async Task SyncMatchesAsync(TeamDataSnapshot snapshot)
    {
        try
        {
            var rawList = ExtractFixtures(snapshot);
            var cleanList = rawList.ToCleanList(snapshot.TeamId);
            cleanList = cleanList
                    .OrderBy(x => x.MatchDate)
                    .ToList();
            if (cleanList.Count == 0)
            {
                _logger.LogInformation("Team {TeamId}: không có match hợp lệ để upsert.", snapshot.TeamId);
                return;
            }

            await EnsureCompetitionsAsync(cleanList);

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

                incoming.LastUpdated = now;
                toUpsert.Add(incoming);
            }

            if (toUpsert.Count > 0)
            {
                await _supabase
                    .From<MatchClean>()
                    .Upsert(
                        toUpsert,
                        new() { OnConflict = "match_id" });

                _logger.LogInformation(
                    "Team {TeamId}: đã upsert {Upserted} match (bỏ qua không đổi: {Skipped}).",
                    snapshot.TeamId,
                    toUpsert.Count,
                    skipped);
            }
            else
            {
                _logger.LogInformation(
                    "Team {TeamId}: không upsert — {Total} match đã khớp DB (bỏ qua {Skipped}).",
                    snapshot.TeamId,
                    cleanList.Count,
                    skipped);
            }

            await _lineupBackfillService.SyncMissingAsync(snapshot.TeamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi sync matches cho team {TeamId}", snapshot.TeamId);
        }
    }



    /// Helper

    private async Task EnsureCompetitionsAsync(List<MatchClean> matches)
    {
        var competitions = matches
            .Where(m => m.CompetitionId.HasValue)
            .GroupBy(m => m.CompetitionId!.Value)
            .Select(g => new CompetitionClean
            {
                CompetitionId = g.Key,
                Name = g.Select(m => m.CompetitionName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                       ?? $"Competition {g.Key}",
                // DB: code NOT NULL — Fotmob không có mã riêng, dùng id làm mã ổn định.
                Code = g.Key.ToString(CultureInfo.InvariantCulture),
                LastUpdated = DateTime.UtcNow
            })
            .ToList();

        if (competitions.Count == 0)
            return;

        await _supabase
            .From<CompetitionClean>()
            .Upsert(competitions, new() { OnConflict = "competition_id" });
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
        return incoming.TeamId == existing.TeamId
            && incoming.OpponentTeamId == existing.OpponentTeamId
            && string.Equals(incoming.OpponentName, existing.OpponentName, StringComparison.Ordinal)
            && incoming.CompetitionId == existing.CompetitionId
            && string.Equals(incoming.CompetitionName, existing.CompetitionName, StringComparison.Ordinal)
            && incoming.MatchDate == existing.MatchDate
            && string.Equals(incoming.HomeOrAway, existing.HomeOrAway, StringComparison.Ordinal)
            && incoming.ScoreHome == existing.ScoreHome
            && incoming.ScoreAway == existing.ScoreAway
            && string.Equals(incoming.Status, existing.Status, StringComparison.Ordinal);
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
