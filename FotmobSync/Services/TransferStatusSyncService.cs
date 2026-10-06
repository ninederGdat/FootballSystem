using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using FotmobSync.Services;
using Supabase.Postgrest;

public class TransferStatusSyncService : ITransferStatusSyncService
{
    private readonly Supabase.Client _supabase;
    private readonly IPlayerStubService _stubService;
    private readonly ILogger<TransferStatusSyncService> _logger;

    public TransferStatusSyncService(
        SupabaseClientFactory factory,
        IPlayerStubService stubService,
        ILogger<TransferStatusSyncService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _stubService = stubService;
        _logger = logger;
    }

    public async Task SyncAsync(
        IEnumerable<PlayerTransferStatus> statuses,
        CancellationToken cancellationToken = default)
    {
        var list = statuses
            .GroupBy(s => s.PlayerId)
            .Select(g => g.Last())
            .ToList();
        if (list.Count == 0) return;

        var existing = await LoadExistingAsync(list.Select(s => s.PlayerId));

        // Player chưa có trong DB: tạo stub thay vì bỏ qua.
        var missing = list.Where(s => !existing.ContainsKey(s.PlayerId)).ToList();
        if (missing.Count > 0)
        {
            foreach (var group in missing.GroupBy(s => s.ParentClubId))
            {
                await _stubService.EnsureAsync(
                    group.Select(s => new PlayerStubInput(
                        s.PlayerId, s.PlayerName, null, null, null)),
                    group.Key);
            }

            foreach (var kv in await LoadExistingAsync(missing.Select(s => s.PlayerId)))
                existing[kv.Key] = kv.Value;

            _logger.LogInformation(
                "TransferStatusSync: created stubs for {Count} players.", missing.Count);
        }

        int updated = 0, unchanged = 0, failed = 0;

        foreach (var resolved in list)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!existing.TryGetValue(resolved.PlayerId, out var player))
            {
                failed++;
                _logger.LogWarning(
                    "TransferStatusSync: player {PlayerId} ({PlayerName}) still missing after stub creation.",
                    resolved.PlayerId, resolved.PlayerName);
                continue;
            }

            if (string.Equals(player.TransferStatus, resolved.status, StringComparison.Ordinal)
                && player.TeamId == resolved.ParentClubId
                && player.CurrentTeamId == resolved.CurrentClubId)
            {
                unchanged++;
                continue;
            }

            try
            {
                await _supabase.From<PlayerClean>()
                    .Filter("player_id", Constants.Operator.Equals, resolved.PlayerId)
                    .Set(x => x.TransferStatus, resolved.status)
                    .Set(x => x.TeamId, resolved.ParentClubId)
                    .Set(x => x.CurrentTeamId, resolved.CurrentClubId)
                    .Update();
                updated++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex,
                    "Failed syncing transfer status for player {PlayerId}", resolved.PlayerId);
            }
        }

        _logger.LogInformation(
            "TransferStatusSync finished: {Updated} updated, {Unchanged} unchanged, {Failed} failed.",
            updated, unchanged, failed);
    }

    private async Task<Dictionary<long, PlayerClean>> LoadExistingAsync(IEnumerable<long> ids)
    {
        var result = new Dictionary<long, PlayerClean>();

        foreach (var chunk in ids.Distinct().Chunk(100))
        {
            var response = await _supabase.From<PlayerClean>()
                .Select("player_id, team_id, current_team_id, transfer_status")
                .Filter("player_id", Constants.Operator.In,
                        chunk.Select(x => (object)x).ToList())
                .Get();

            foreach (var p in response.Models)
                result[p.PlayerId] = p;
        }

        return result;
    }
}