using System.Text.Json;
using FotmobSync.Models.Raw;

namespace FotmobSync.Modules;

/// <summary>
/// Một lần gọi team API: root JSON (độc lập với <see cref="JsonDocument"/>) và <see cref="TeamRaw"/> đã deserialize.
/// </summary>
public sealed class TeamDataSnapshot
{
    public TeamDataSnapshot(int teamId, JsonElement root, TeamRaw? teamRaw)
    {
        TeamId = teamId;
        Root = root;
        TeamRaw = teamRaw;
    }

    public int TeamId { get; }

    /// <summary>Clone từ <c>JsonDocument.RootElement</c>; an toàn sau khi document bị dispose.</summary>
    public JsonElement Root { get; }

    public TeamRaw? TeamRaw { get; }
}
