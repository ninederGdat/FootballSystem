using FotmobSync.Models.Sync;
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
    TeamDataSnapshot snapshot,
    CancellationToken cancellationToken = default)
{
    var players = ExtractPlayers(snapshot);

    foreach (var player in players)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation(
                "Syncing player {PlayerId} - {PlayerName}",
                player.PlayerId,
                player.Name);

            await _playerSyncService.SyncAsync(
                player.PlayerId,
                player.TeamId,
                cancellationToken);

            await Task.Delay(
                TimeSpan.FromSeconds(6),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed syncing player {PlayerId}",
                player.PlayerId);
        }
    }
}

    private static List<SquadPlayerRef> ExtractPlayers(
        TeamDataSnapshot snapshot)
    {
        var squad = snapshot.TeamRaw?.Squad;

        if (squad == null)
            return [];

        return squad.Groups
            // Bỏ nhóm Coach
            .Where(g => !string.Equals(g.Title, "Coach",
                StringComparison.OrdinalIgnoreCase))
            .SelectMany(g => g.Members)
            .Select(p => new SquadPlayerRef(
                p.Id,
                snapshot.TeamId,
                p.Name ?? string.Empty))
            .ToList();
    }
}