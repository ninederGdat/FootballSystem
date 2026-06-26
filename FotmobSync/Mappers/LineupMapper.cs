using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;

namespace FotmobSync.Mappers;

/// <summary>
/// Convert FotMob lineup raw data
/// → clean lineup entities
/// for club-centric architecture.
///
/// Current design:
/// - 1 match
/// - 1 lineup row
/// - only followed club players
/// </summary>
public static class LineupMapper
{
    /// <summary>
    /// Convert MatchLineupRaw
    /// → LineupClean
    /// </summary>
    public static LineupClean ToClean(
        this MatchLineupRaw? raw,
        long formationId)
    {
        return new LineupClean
        {
            MatchId = raw?.MatchId ?? 0,
            // FotMob lineup type: standard, etc...
            Type = raw?.LineupType ?? "standard",
            FormationId = formationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Convert collection
    /// → clean lineup list
    /// </summary>
    public static List<LineupClean> ToCleanList(
        this IEnumerable<MatchLineupRaw> raws,
        long formationId)
    {
        return raws
            .Select(x => x.ToClean(formationId))
            .ToList();
    }

    /// <summary>
    /// Get followed club formation.
    ///
    /// Example:
    /// "4-2-3-1"
    /// </summary>
    public static string GetFormation(
        this MatchLineupRaw? raw,
        long targetTeamId)
    {
        if (raw is null)
        {
            return string.Empty;
        }
        if (raw.HomeTeam?.TeamId == targetTeamId)
        {
            return NormalizeFormation(
                raw.HomeTeam.Formation);
        }
        if (raw.AwayTeam?.TeamId == targetTeamId)
        {
            return NormalizeFormation(
                raw.AwayTeam.Formation);
        }
        return string.Empty;
    }

    /// <summary>
    /// Get starters of followed club.
    /// </summary>
    public static IEnumerable<LineupPlayerRaw> GetStarters(
        this MatchLineupRaw? raw,
        long targetTeamId)
    {
        if (raw is null)
        {
            return Enumerable.Empty<LineupPlayerRaw>();
        }
        if (raw.HomeTeam?.TeamId == targetTeamId)
        {
            return raw.HomeTeam.Starters
                   ?? Enumerable.Empty<LineupPlayerRaw>();
        }
        if (raw.AwayTeam?.TeamId == targetTeamId)
        {
            return raw.AwayTeam.Starters
                   ?? Enumerable.Empty<LineupPlayerRaw>();
        }
        return Enumerable.Empty<LineupPlayerRaw>();
    }

    /// <summary>
    /// Get substitutes of followed club.
    /// </summary>
    public static IEnumerable<LineupPlayerRaw> GetSubstitutes(
        this MatchLineupRaw? raw,
        long targetTeamId)
    {
        if (raw is null)
        {
            return Enumerable.Empty<LineupPlayerRaw>();
        }
        if (raw.HomeTeam?.TeamId == targetTeamId)
        {
            return raw.HomeTeam.Subs
                   ?? Enumerable.Empty<LineupPlayerRaw>();
        }
        if (raw.AwayTeam?.TeamId == targetTeamId)
        {
            return raw.AwayTeam.Subs
                   ?? Enumerable.Empty<LineupPlayerRaw>();
        }
        return Enumerable.Empty<LineupPlayerRaw>();
    }

    /// <summary>
    /// Get all lineup players
    /// (starters + substitutes)
    /// of followed club.
    /// </summary>
    public static IEnumerable<LineupPlayerRaw> GetAllPlayers(
        this MatchLineupRaw? raw,
        long targetTeamId)
    {
        return raw.GetStarters(targetTeamId)
            .Concat(raw.GetSubstitutes(targetTeamId));
    }

    /// <summary>
    /// Normalize formation string
    /// before DB lookup.
    /// </summary>
    private static string NormalizeFormation(
        string? formation)
    {
        if (string.IsNullOrWhiteSpace(formation))
        {
            return string.Empty;
        }

        return formation.Trim();
    }

   
}