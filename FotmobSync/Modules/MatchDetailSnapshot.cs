using System.Text.Json;
using FotmobSync.Models.Raw;

namespace FotmobSync.Modules;

public sealed class MatchDetailSnapshot
{
    public MatchDetailSnapshot(
        int matchId,
        int teamId,
        JsonElement root,
        MatchLineupRaw? lineupRaw,
        List<MatchEventRaw>? matchEventsRaw = null)
    {
        MatchId = matchId;
        TeamId = teamId;
        Root = root;
        LineupRaw = lineupRaw;
        MatchEventsRaw = matchEventsRaw ?? new List<MatchEventRaw>();
    }

    public int MatchId { get; }

    public int TeamId { get; }

    public JsonElement Root { get; }

    public MatchLineupRaw? LineupRaw { get; }

    /// <summary>
    /// content.matchFacts.events.events[] đã parse — cùng response với LineupRaw,
    /// </summary>
    public List<MatchEventRaw> MatchEventsRaw { get; }
}