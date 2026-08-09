using System.Text.Json;
using FootballApi.DTOs.Responses;
using FootballApi.Repositories.MatchEvent;
using FootballApi.Repositories.Player;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Services.MatchEvent;

public class MatchEventService : IMatchEventService
{
    private readonly IMatchEventRepository _eventRepository;
    private readonly IPlayerRepository _playerRepository;

    public MatchEventService(IMatchEventRepository eventRepository, IPlayerRepository playerRepository)
    {
        _eventRepository = eventRepository;
        _playerRepository = playerRepository;
    }

    public async Task<List<MatchEventItemResponse>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default)
    {
        var events = await _eventRepository.GetEventsByMatchIdAsync(matchId, ct);
        if (events.Count == 0) return [];

        var playerIds = events
            .SelectMany(e => new[] { e.PlayerId, e.AssistPlayerId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var playersById = playerIds.Count > 0
            ? (await _playerRepository.GetByIdsAsync(playerIds, ct))
                .GroupBy(p => p.PlayerId)
                .ToDictionary(g => g.Key, g => g.First())
            : [];

        return events.Select(e => new MatchEventItemResponse(
            Id: e.Id,
            EventId: e.FotmobEventId,
            Type: e.EventType,
            Minute: e.Minute,
            StoppageTime: e.StoppageTime,
            TeamId: e.TeamId,
            Player: BuildPlayerRef(e.PlayerId, e.RawPayload, "player", playersById),
            Assist: BuildPlayerRef(e.AssistPlayerId, e.RawPayload, "assist", playersById),
            Description: e.DescriptionKey
        )).ToList();
    }

    private static PlayerRef? BuildPlayerRef(
        long? playerId, string rawPayload, string rawKey,
        Dictionary<long, PlayerClean> playersById)
    {
        if (playerId.HasValue)
        {
            var name = playersById.GetValueOrDefault(playerId.Value)?.Name
                       ?? GetFallbackName(rawPayload, rawKey);
            return new PlayerRef(playerId.Value, name);
        }

        // player_id null (thường là đội khách) — vẫn thử hiển thị tên từ raw_payload nếu có
        var fallbackName = GetFallbackName(rawPayload, rawKey);
        return fallbackName is not null ? new PlayerRef(null, fallbackName) : null;
    }

    private static string? GetFallbackName(string rawPayload, string propertyKey)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawPayload);
            if (doc.RootElement.TryGetProperty(propertyKey, out var el) &&
                el.TryGetProperty("name", out var nameEl))
            {
                return nameEl.GetString();
            }
        }
        catch (JsonException) { }
        return null;
    }
}