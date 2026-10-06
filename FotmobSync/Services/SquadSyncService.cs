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
        IReadOnlyCollection<TeamDataSnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        var players = snapshots
            .SelectMany(ExtractPlayers)
            .GroupBy(p => p.PlayerId)
            .Select(g => g.First())
            .ToList();

        _logger.LogInformation(
            "Squad sync: {Count} distinct players from {Teams} teams.",
            players.Count, snapshots.Count);

        int ok = 0, failed = 0;

        foreach (var player in players)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Syncing player {PlayerId} - {PlayerName}",
                player.PlayerId, player.Name);

            var success = await _playerSyncService.SyncAsync(
                player.PlayerId,
                player.TeamId,
                 useProfileTeam: false,
                cancellationToken
               );   // đổi thành true sau khi kiểm tra primaryTeam của cầu thủ cho mượn

            if (success) ok++;
            else failed++;

            await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken);
        }

        _logger.LogInformation(
            "Squad sync finished: {Ok} ok, {Failed} failed.", ok, failed);
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

    public Task SyncAsync(TeamDataSnapshot snapshot, CancellationToken cancellationToken = default)
    => SyncAsync(new[] { snapshot }, cancellationToken);
}