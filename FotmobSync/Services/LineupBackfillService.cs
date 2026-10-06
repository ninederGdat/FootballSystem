using FootballSystem.Shared.Infrastructure;
using FotmobSync.Workflows;
using FootballSystem.Shared.Models.Clean;
using Supabase.Postgrest;

namespace FotmobSync.Services;

public class LineupBackfillService : ILineupBackfillService
{
    private readonly Supabase.Client _supabase;

    private readonly MatchLineupWorkflow _workflow;

    private readonly ILogger<LineupBackfillService> _logger;

    public LineupBackfillService(
        SupabaseClientFactory factory,
        MatchLineupWorkflow workflow,
        ILogger<LineupBackfillService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _workflow = workflow;
        _logger = logger;
    }

    private async Task<List<MatchClean>> LoadMissingMatchesAsync(int teamId)
    {
        var finished = (await _supabase.From<MatchClean>()
     .Filter("team_id", Constants.Operator.Equals, teamId)
     .Filter("status", Constants.Operator.Equals, "FINISHED")
     .Get()).Models;
        if (finished.Count == 0) return new();

        var completeIds = new HashSet<long>();
        var attempts = new Dictionary<long, LineupSyncAttemptClean>();

        foreach (var chunk in finished.Select(m => (object)m.MatchId).Chunk(200))
        {
            var ids = chunk.ToList();

            var done = await _supabase.From<LineupClean>()
                .Filter("match_id", Constants.Operator.In, ids)
                .Filter("is_complete", Constants.Operator.Equals, "true")
                .Get();
            foreach (var l in done.Models) completeIds.Add(l.MatchId);

            var tried = await _supabase.From<LineupSyncAttemptClean>()
                .Filter("match_id", Constants.Operator.In, ids)
                .Get();
            foreach (var a in tried.Models) attempts[a.MatchId] = a;
        }

        var cutoff = DateTime.UtcNow.AddHours(-6);
        return finished
            .Where(m => !completeIds.Contains(m.MatchId))
            .Where(m => !attempts.TryGetValue(m.MatchId, out var a)
                        || a.Attempts < 5 || a.LastAttemptAt < cutoff)
            .OrderBy(m => m.MatchDate)
            .ToList();
    }

    public async Task SyncMissingAsync(int teamId)
    {
        var matches = await LoadMissingMatchesAsync(teamId);
        _logger.LogInformation("Found {Count} matches with missing/incomplete lineup.", matches.Count);

        foreach (var match in matches)
        {
            var ok = false;
            try { ok = await _workflow.ExecuteAsync((int)match.MatchId, teamId); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backfill failed for match {MatchId}", match.MatchId);
            }
            finally { await RecordAttemptAsync(match.MatchId, ok); }
        }
    }

    private async Task RecordAttemptAsync(long matchId, bool success)
    {
        try
        {
            if (success)
            {
                await _supabase
                    .From<LineupSyncAttemptClean>()
                    .Filter("match_id", Constants.Operator.Equals, matchId)
                    .Delete();
                return;
            }

            var existing = (await _supabase
                .From<LineupSyncAttemptClean>()
                 .Filter("match_id", Constants.Operator.Equals, matchId)
                .Get()).Models.FirstOrDefault();

            await _supabase
                .From<LineupSyncAttemptClean>()
                .Upsert(
                    new LineupSyncAttemptClean
                    {
                        MatchId = matchId,
                        Attempts = (existing?.Attempts ?? 0) + 1,
                        LastAttemptAt = DateTime.UtcNow
                    },
                    new() { OnConflict = "match_id" });
        }
        catch (Exception ex)
        {
            // Ghi nhận attempt lỗi không được làm hỏng vòng backfill.
            _logger.LogError(ex, "Cannot record lineup attempt for match {MatchId}", matchId);
        }
    }
}
