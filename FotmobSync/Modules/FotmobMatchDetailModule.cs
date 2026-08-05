using System.Text.Json;
using FotmobSync.Clients;
using FotmobSync.Models.Raw;

namespace FotmobSync.Modules;

public class FotmobMatchDetailModule
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly FotmobClient _client;
    private readonly ILogger<FotmobMatchDetailModule> _logger;

    public FotmobMatchDetailModule(
        FotmobClient client,
        ILogger<FotmobMatchDetailModule> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<MatchDetailSnapshot> LoadAsync(int matchId, int teamId)
    {
        _logger.LogDebug(
            "Loading match detail {MatchId}",
            matchId);

        using var doc =
            await _client.GetMatchDetailDataAsync(matchId);

        var root =
            doc.RootElement.Clone();

        MatchLineupRaw? lineupRaw = null;
        List<MatchEventRaw> matchEventsRaw = new();

        if (root.TryGetProperty("content", out var content))
        {
            if (content.TryGetProperty("lineup", out var lineup))
            {
                lineupRaw =
                    JsonSerializer.Deserialize<MatchLineupRaw>(
                        lineup.GetRawText(),
                        JsonOptions);
            }

            matchEventsRaw = ParseMatchEvents(content, matchId);
        }
        else
        {
            _logger.LogWarning(
                "Match detail {MatchId}: missing 'content' node, skipping lineup and events parsing",
                matchId);
        }

        return new MatchDetailSnapshot(
            matchId,
            teamId,
            root,
            lineupRaw,
            matchEventsRaw);
    }

    /// <summary>
    /// content.matchFacts nằm cùng cấp với content.lineup trong cùng response
    /// Parse content.matchFacts.events.events[], gán SourceIndex theo thứ tự mảng gốc
    /// và giữ nguyên RawJson của từng phần tử để phục vụ cột raw_payload sau này.
    /// </summary>
    private List<MatchEventRaw> ParseMatchEvents(JsonElement content, int matchId)
    {
        var result = new List<MatchEventRaw>();

        if (!content.TryGetProperty("matchFacts", out var matchFacts))
        {
            _logger.LogDebug(
                "Match detail {MatchId}: no 'matchFacts' node, no events to parse",
                matchId);
            return result;
        }

        if (!matchFacts.TryGetProperty("events", out var eventsWrapper) ||
            !eventsWrapper.TryGetProperty("events", out var eventsArray) ||
            eventsArray.ValueKind != JsonValueKind.Array)
        {
            _logger.LogDebug(
                "Match detail {MatchId}: no 'matchFacts.events.events' array, no events to parse",
                matchId);
            return result;
        }

        var index = 0;
        foreach (var element in eventsArray.EnumerateArray())
        {
            try
            {
                var raw = element.Deserialize<MatchEventRaw>(JsonOptions);
                if (raw is null)
                {
                    _logger.LogWarning(
                        "Match detail {MatchId}: failed to deserialize event at index {Index}, skipping",
                        matchId, index);
                    index++;
                    continue;
                }

                // Clone vì element gốc thuộc JsonDocument sẽ bị dispose (using ở LoadAsync)
                raw.RawJson = element.Clone();
                raw.SourceIndex = index;
                result.Add(raw);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    """
                    Match detail {MatchId}: exception deserializing event at index {Index},                     
                    Path    : {Path},
                     Event: {EventJson},
                    skipping
                    """,
                    matchId,
                    index,
                    ex.Path,
                    element.GetRawText());
            }

            index++;
        }

        return result;
    }
}
