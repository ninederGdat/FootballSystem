using FootballSystem.Shared.Infrastructure;
using FotmobSync.Infrastructure.External.Fotmob.Mapping;
using FotmobSync.Infrastructure.Resolvers.PlayingTime;
using FotmobSync.Infrastructure.Resolvers.PositionRole;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Modules;
using FotmobSync.Services;

public class LineupPlayerSyncService : ILineupPlayerSyncService
{
    private readonly Supabase.Client _supabase;
    private readonly IFotmobPositionMapper _positionMapper;
    private readonly IPositionRoleResolver _positionRoleResolver;
    private readonly IPlayingTimeResolver _playingTimeResolver;
    private readonly ILogger<LineupPlayerSyncService>
        _logger;

    public LineupPlayerSyncService(
        SupabaseClientFactory factory,
        IFotmobPositionMapper positionMapper,
        IPositionRoleResolver positionRoleResolver,
        IPlayingTimeResolver playingTimeResolver,
        ILogger<LineupPlayerSyncService> logger)
    {
        _supabase =
            factory.CreateServiceRoleClient();
        _positionMapper = positionMapper;
        _positionRoleResolver = positionRoleResolver;
        _playingTimeResolver = playingTimeResolver;
        _logger = logger;
    }


    public async Task SyncAsync(
        MatchDetailSnapshot snapshot,
        long lineupId)
    {
        var team = GetTeam(snapshot);

        if (team is null)
            return;

        var players = BuildPlayers(team).ToList();

        if (players.Count == 0)
            return;

        var existingIds = await GetExistingPlayerIdsAsync(players);

        var entities = new List<LineupPlayerUpsert>();

        foreach (var item in players)
        {
            var entity = CreateLineupPlayer(
                item,
                lineupId,
                existingIds);

            if (entity != null)
                entities.Add(entity);
        }

        await UpsertPlayersAsync(entities);
    }



    // ------------------------------------------------------------
    // ETL Pipeline Helpers
    // ------------------------------------------------------------

    private async Task<HashSet<long>> GetExistingPlayerIdsAsync(
        IEnumerable<(LineupPlayerRaw Player, bool IsStarter)> players)
    {
        var ids = players
            .Select(x => x.Player.PlayerId)
            .Distinct()
            .ToList();

        var response = await _supabase
            .From<PlayerClean>()
            .Filter(
                "player_id",
                Supabase.Postgrest.Constants.Operator.In,
                ids)
            .Get();

        return response.Models
            .Select(x => x.PlayerId)
            .ToHashSet();
    }


    private LineupPlayerUpsert? CreateLineupPlayer(
        (LineupPlayerRaw Player, bool IsStarter) item,
        long lineupId,
        HashSet<long> existingIds)
    {
        var p = item.Player;

        if (!existingIds.Contains(p.PlayerId))
        {
            _logger.LogWarning(
                "Player {PlayerId} not found.",
                p.PlayerId);

            return null;
        }

        var positionCode = ResolvePositionCode(p);

        var roleId = ResolveRoleId(positionCode);

        var playingTime =
            _playingTimeResolver.Resolve(
                p,
                item.IsStarter);

        var entity = new LineupPlayerUpsert
        {
            LineupId = lineupId,
            PlayerId = p.PlayerId,
            ShirtNumber = ParseShirtNumber(p.ShirtNumber),
            IsStarter = item.IsStarter,
            MinuteIn = playingTime.MinuteIn,
            MinuteOut = playingTime.MinuteOut,
            PositionCode = positionCode,
            RoleId = roleId,
            CustomX = p.HorizontalLayout?.X,
            CustomY = p.HorizontalLayout?.Y
        };

        LogPlayer(
            p,
            positionCode,
            roleId);

        return entity;
    }

    private async Task UpsertPlayersAsync(
    ICollection<LineupPlayerUpsert> entities)
    {
        await _supabase
            .From<LineupPlayerUpsert>()
            .Upsert(
                entities,
                new()
                {
                    OnConflict = "lineup_id,player_id"
                });
    }


    // ------------------------------------------------------------
    // Domain Helpers
    // ------------------------------------------------------------

    private string? ResolvePositionCode(
        LineupPlayerRaw player)
    {
        if (!player.PositionId.HasValue)
            return null;

        if (_positionMapper.TryMap(
            player.PositionId.Value,
            out var positionCode))
        {
            return positionCode;
        }

        _logger.LogWarning(
            "Unknown PositionId {PositionId}",
            player.PositionId.Value);

        return null;
    }

    private int? ResolveRoleId(
        string? positionCode)
    {
        if (_positionRoleResolver.TryResolveRoleId(
            positionCode,
            out var roleId))
        {
            return roleId;
        }

        _logger.LogWarning(
            "Unknown PositionCode {PositionCode}",
            positionCode);

        return null;
    }


    private void LogPlayer(
        LineupPlayerRaw player,
        string? positionCode,
        int? roleId)
    {
        _logger.LogInformation(
            "Player: {Name} ({PlayerId}) | PositionId={PositionId} | PositionCode={PositionCode} | RoleId={RoleId} | UsualPositionId={UsualPositionId} | X={X} | Y={Y}",
            player.Name,
            player.PlayerId,
            player.PositionId,
            positionCode,
            roleId,
            player.UsualPlayingPositionId,
            player.HorizontalLayout?.X,
            player.HorizontalLayout?.Y);
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