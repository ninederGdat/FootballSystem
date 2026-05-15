using System.Text.Json;
using FotmobSync.Clients;
using FotmobSync.Models.Raw;
using Microsoft.Extensions.Logging;

namespace FotmobSync.Modules;

/// <summary>
/// Gom một chỗ gọi FotMob team API (<c>/api/data/teams</c>) để tái sử dụng payload cho team, squad và fixtures.
/// </summary>
public class FotmobTeamDataModule
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly FotmobClient _client;
    private readonly ILogger<FotmobTeamDataModule> _logger;

    public FotmobTeamDataModule(FotmobClient client, ILogger<FotmobTeamDataModule> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Tải dữ liệu đội một lần. <see cref="TeamDataSnapshot.Root"/> không phụ thuộc document gốc.
    /// </summary>
    public async Task<TeamDataSnapshot> LoadAsync(int teamId)
    {
        _logger.LogDebug("FotmobTeamDataModule: loading team {TeamId}", teamId);

        using var doc = await _client.GetTeamDataAsync(teamId);
        var root = doc.RootElement.Clone();
        var teamRaw = JsonSerializer.Deserialize<TeamRaw>(root, JsonOptions);
        return new TeamDataSnapshot(teamId, root, teamRaw);
    }
}
