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


        if (root.TryGetProperty("content", out var content) &&
            content.TryGetProperty("lineup", out var lineup))
        {
            lineupRaw =
                JsonSerializer.Deserialize<MatchLineupRaw>(
                    lineup.GetRawText(),
                    JsonOptions);
        }

        
        return new MatchDetailSnapshot(
            matchId,
            teamId,
            root,
            lineupRaw);
    }
}