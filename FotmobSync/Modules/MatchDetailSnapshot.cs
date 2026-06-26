using System.Text.Json;
using FotmobSync.Models.Raw;

namespace FotmobSync.Modules;

public sealed class MatchDetailSnapshot
{
    public MatchDetailSnapshot(
        int matchId,
        int teamId,
        JsonElement root,
        MatchLineupRaw? lineupRaw)
    {
        MatchId = matchId;
        TeamId = teamId;
        Root = root;
        LineupRaw = lineupRaw;
    }

    public int MatchId { get; }

    public int TeamId { get; }

    public JsonElement Root { get; }

    public MatchLineupRaw? LineupRaw { get; }
}