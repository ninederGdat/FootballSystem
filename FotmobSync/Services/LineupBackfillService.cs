using FotmobSync.Infrastructure;
using FotmobSync.Models.Clean;
using FotmobSync.Workflows;

namespace FotmobSync.Services;

public class LineupBackfillService : ILineupBackfillService
{
    private readonly Supabase.Client _supabase;

    private readonly MatchLineupWorkflow _workflow;

    private readonly ILogger<LineupBackfillService>_logger;

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
        var matches = await _supabase
            .From<MatchClean>()
            .Get();

        var lineups = await _supabase
            .From<LineupClean>()
            .Get();

        var lineupMatchIds =
            lineups.Models
                   .Select(x => x.MatchId)
                   .ToHashSet();

        return matches.Models
            .Where(x =>x.TeamId == teamId &&
                x.Status == "FINISHED" &&
                !lineupMatchIds.Contains(x.MatchId))
            .OrderBy(x => x.MatchDate)
            .ToList();
    }

    public async Task SyncMissingAsync(
        int teamId)
    {
        var matches =await LoadMissingMatchesAsync(teamId);

        _logger.LogInformation(
            "Found {Count} matches without lineup.",
            matches.Count);

        foreach (var match in matches)
        {
            try
            {
                _logger.LogInformation(
                    "Backfill lineup for match {MatchId}",
                    match.MatchId);

                await _workflow.ExecuteAsync(
                    (int)match.MatchId,
                    teamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Backfill failed for match {MatchId}",
                    match.MatchId);
            }
        }

        _logger.LogInformation(
            "Lineup backfill completed.");
    }
}
