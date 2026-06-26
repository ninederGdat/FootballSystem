using System.Text.Json;
using FotmobSync.Models.Clean;
using FotmobSync.Modules;
using FotmobSync.Services;

public class SquadSyncService : ISquadSyncService
{
    private readonly IPlayerSyncService _playerSyncService;
    private readonly ILogger<SquadSyncService> _logger;

    public SquadSyncService(
        IPlayerSyncService playerSyncService,
        ILogger<SquadSyncService> logger)
    {
        _playerSyncService = playerSyncService;
        _logger = logger;
    }

    public async Task SyncAsync(
        TeamDataSnapshot snapshot)
    {
        var players =
            ExtractPlayers(snapshot);

        foreach (var player in players)
        {
            await _playerSyncService.SyncAsync(
                (int)player.PlayerId,
                player.TeamId);

            await Task.Delay(6000);
        }
    }

    private List<PlayerClean> ExtractPlayers(
        TeamDataSnapshot snapshot)
    {
        var rawTeam = snapshot.TeamRaw;

        if (rawTeam?.Squad == null)
            return [];

        var players = new List<PlayerClean>();

        foreach (var group in rawTeam.Squad.Groups)
        {
            foreach (var p in group.Members)
            {
                players.Add(new PlayerClean
                {
                    PlayerId = p.Id,
                    TeamId = snapshot.TeamId,
                    Name = p.Name ?? string.Empty,
                    ShirtNumber = ParseShirtNumber(p.ShirtNumber),
                    Nationality = p.CountryCode
                });
            }
        }

        return players;
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