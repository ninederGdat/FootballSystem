using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Mappers;
using FotmobSync.Modules;
using Supabase.Postgrest;

namespace FotmobSync.Services;

public class MatchEventSyncService : IMatchEventSyncService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<MatchEventSyncService> _logger;

    public MatchEventSyncService(
        SupabaseClientFactory factory,
        ILogger<MatchEventSyncService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task SyncAsync(
        MatchDetailSnapshot snapshot,
        long matchId)
    {
        var rawEvents = snapshot.MatchEventsRaw;
        if (rawEvents is null || rawEvents.Count == 0)
            return;

        var homeTeamId = (long)(snapshot.LineupRaw?.HomeTeam?.TeamId ?? 0);
        var awayTeamId = (long)(snapshot.LineupRaw?.AwayTeam?.TeamId ?? 0);
        var followedTeamId = snapshot.TeamId;

        // MatchEventRaw: player ghi/nhận event nằm ở raw.Player?.Id (scorer)
        // và raw.AssistPlayerId (assist), không phải field phẳng "PlayerId".
        var referencedPlayerIds = rawEvents
            .SelectMany(e => new[] { e.Player?.Id, e.AssistPlayerId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var knownPlayerIds = await GetKnownPlayerIdsAsync(referencedPlayerIds);

        var events = rawEvents.ToUpsertList(
            matchId,
            homeTeamId,
            awayTeamId,
            followedTeamId,
            knownPlayerIds);

        if (events.Count < rawEvents.Count)
        {
            _logger.LogWarning(
                "Match {MatchId}: {Skipped}/{Total} event bị bỏ qua (type ngoài phạm vi Goal/Card).",
                matchId,
                rawEvents.Count - events.Count,
                rawEvents.Count);
        }

        _logger.LogInformation(
            "Mapped {Count} match events for match {MatchId} ({KnownPlayers} known players out of {ReferencedPlayers} referenced)",
            events.Count,
            matchId,
            knownPlayerIds.Count,
            referencedPlayerIds.Count);

        if (events.Count == 0)
            return;

        await UpsertAsync(events, matchId);
    }

    /// <summary>
    /// Upsert match events theo fotmob_event_id.
    /// Mỗi event là 1 row độc lập (goal/card), khác LineupSyncService
    /// (chỉ 1 row/match) nên dùng Upsert danh sách thay vì Single().
    /// </summary>
    private async Task UpsertAsync(
        List<MatchEventUpsert> events,
        long matchId)
    {
        try
        {
            await _supabase
                .From<MatchEventUpsert>()
                .Upsert(events, new()
                {
                    OnConflict = "fotmob_event_id"
                });

            _logger.LogInformation(
                "Upserted {Count} match events for match {MatchId}.",
                events.Count,
                matchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error upserting match events for match {MatchId}",
                matchId);
        }
    }

    /// <summary>
    /// Query trực tiếp player_id đã tồn tại trong DB,
    /// dùng để lọc — không sync tối thiểu cầu thủ đối phương.
    /// </summary>
    private async Task<HashSet<long>> GetKnownPlayerIdsAsync(
        List<long> referencedPlayerIds)
    {
        if (referencedPlayerIds.Count == 0)
            return new HashSet<long>();

        var response = await _supabase
            .From<PlayerClean>()
            .Filter("player_id", Constants.Operator.In, referencedPlayerIds)
            .Get();

        return (response.Models ?? new List<PlayerClean>())
            .Select(p => p.PlayerId)
            .ToHashSet();
    }
}