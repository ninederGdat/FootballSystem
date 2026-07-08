using System.Text.Json;
using FootballSystem.Shared.Infrastructure;
using FotmobSync.Clients;
using FotmobSync.Infrastructure;
using FotmobSync.Mappers;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Modules;
using FotmobSync.Services;
namespace FotmobSync.Services;


public class LineupSyncService : ILineupSyncService
{
    private readonly FormationService _formationService;
    private readonly ILogger<LineupSyncService> _logger;
    private readonly Supabase.Client _supabase;

    public LineupSyncService(
     SupabaseClientFactory factory,
     FormationService formationService,
     ILogger<LineupSyncService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _formationService = formationService;
        _logger = logger;
    }

    public async Task<LineupClean?> SyncAsync(
        MatchDetailSnapshot snapshot)
    {
        var raw = snapshot.LineupRaw;

        if (raw is null)
        {
            _logger.LogInformation(
                "Match {MatchId}: lineup unavailable.",
                snapshot.MatchId);

            return null;
        }

        var formationName =
         raw.GetFormation(snapshot.TeamId);


        if (string.IsNullOrWhiteSpace(
            formationName))
        {
            _logger.LogWarning(
                "Match {MatchId}: formation missing.",
                snapshot.MatchId);

            return null;
        }

        var formationId =
            await _formationService.ResolveAsync(
                formationName);

        if (formationId is null)
        {
            _logger.LogWarning(
                "Formation {Formation} not found.",
                formationName);

            return null;
        }

        var lineup = raw.ToClean(formationId.Value);

        lineup.MatchId = snapshot.MatchId;

        lineup.UpdatedAt = DateTime.UtcNow;

        var lineupUpsert = new LineupUpsert
        {
            MatchId = lineup.MatchId,
            Type = lineup.Type,
            FormationId = lineup.FormationId,
            UpdatedAt = lineup.UpdatedAt
        };

        var response = await _supabase
        .From<LineupUpsert>()
        .Upsert(lineupUpsert, new()
        {
            OnConflict = "match_id"
        });

        var savedLineup = await _supabase
            .From<LineupClean>()
            .Where(x => x.MatchId == snapshot.MatchId)
            .Single();

        _logger.LogInformation(
            """
                SAVED LINEUP
                Id={Id}
                MatchId={MatchId}
                """,
            savedLineup.Id,
            savedLineup.MatchId);

        return savedLineup;
    }


    
}