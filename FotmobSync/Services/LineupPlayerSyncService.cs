using FotmobSync.Infrastructure;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Modules;
using FotmobSync.Services;

public class LineupPlayerSyncService : ILineupPlayerSyncService
{
    private readonly Supabase.Client _supabase;

    private readonly ILogger<LineupPlayerSyncService>
        _logger;

    public LineupPlayerSyncService(
        SupabaseClientFactory factory,
        ILogger<LineupPlayerSyncService> logger)
    {
        _supabase =
            factory.CreateServiceRoleClient();

        _logger = logger;
    }


    public async Task SyncAsync(
    MatchDetailSnapshot snapshot,
    long lineupId)
    {
        var team = GetTeam(snapshot);

        if (team is null)
            return;

        var players =
            BuildPlayers(team).ToList();

        if (players.Count == 0)
            return;

        var playerIds = players
            .Select(x => x.Player.PlayerId)
            .Distinct()
            .ToList();

        var existingPlayers = await _supabase
            .From<PlayerClean>()
            .Filter(
                "player_id",
                Supabase.Postgrest.Constants.Operator.In,
                playerIds)
            .Get();

        var existingIds = existingPlayers.Models
        .Select(x => x.PlayerId)
        .ToHashSet();

        var now = DateTime.UtcNow;

        var entities = new List<LineupPlayerUpsert>();

        foreach (var item in players)
        {
            var p = item.Player;

            if (!existingIds.Contains(p.PlayerId))
            {
                _logger.LogWarning(
                    "Player {PlayerId} not found.",
                    p.PlayerId);

                continue;
            }

            entities.Add(
                new LineupPlayerUpsert
                {
                    LineupId = lineupId,
                    PlayerId = p.PlayerId,
                    ShirtNumber = ParseShirtNumber(p.ShirtNumber),
                    IsStarter = item.IsStarter,
                    PositionCode = null,
                    RoleId = null,
                    CustomX = p.HorizontalLayout?.X,
                    CustomY = p.HorizontalLayout?.Y
                });
        }


        await _supabase
         .From<LineupPlayerUpsert>()
         .Upsert(
             entities,
             new()
             {
                 OnConflict = "lineup_id,player_id"
             });

        _logger.LogInformation(
            "Match {MatchId}: synced {Count} lineup players.",
            snapshot.MatchId,
            entities.Count);

        _logger.LogInformation(
            "LineupPlayerSync lineupId={LineupId}",
            lineupId);
    }


    ///Helper 
    /// Just get 1 data's team
    private static MatchLineupTeamRaw? GetTeam(
    MatchDetailSnapshot snapshot)
    {
        var raw = snapshot.LineupRaw;

        if (raw == null)
            return null;

        if (raw.HomeTeam?.TeamId == snapshot.TeamId)
            return raw.HomeTeam;

        if (raw.AwayTeam?.TeamId == snapshot.TeamId)
            return raw.AwayTeam;

        return null;
    }

    //Helper build player list
    private static IEnumerable<(LineupPlayerRaw Player, bool IsStarter)> BuildPlayers(
        MatchLineupTeamRaw team)
    {
        foreach (var p in team.Starters)
            yield return (p, true);

        foreach (var p in team.Subs)
            yield return (p, false);
    }

    private static int? ParseShirtNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, out var number)
            ? number
            : null;
    }

}