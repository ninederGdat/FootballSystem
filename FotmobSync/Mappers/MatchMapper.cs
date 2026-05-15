using System.Globalization;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;

namespace FotmobSync.Mappers;

/// <summary>
/// Mapper từ <see cref="MatchRaw"/> (fixtures trong team API) sang <see cref="MatchClean"/>.
/// </summary>
public static class MatchMapper
{
    public static MatchClean? ToClean(this MatchRaw? raw)
    {
        if (raw == null || raw.Id == 0 || raw.Home == null || raw.Away == null)
            return null;

        var now = DateTime.UtcNow;
        return new MatchClean
        {
            MatchId = raw.Id,
            HomeTeamId = raw.Home.Id,
            AwayTeamId = raw.Away.Id,
            HomeTeamName = raw.Home.Name?.Trim(),
            AwayTeamName = raw.Away.Name?.Trim(),
            HomeScore = raw.Home.Score,
            AwayScore = raw.Away.Score,
            TournamentName = raw.Tournament?.Name?.Trim(),
            LeagueId = raw.Tournament == null ? null : raw.Tournament.LeagueId,
            KickoffUtc = ParseKickoffUtc(raw.Status?.UtcTime),
            Started = raw.Status?.Started ?? false,
            Finished = raw.Status?.Finished ?? false,
            Cancelled = raw.Status?.Cancelled ?? false,
            CreatedAt = now,
            LastUpdated = now
        };
    }

    public static List<MatchClean> ToCleanList(this IEnumerable<MatchRaw?> rawList) =>
        rawList.Select(r => r.ToClean()).Where(c => c != null).Select(c => c!).ToList();

    private static DateTime? ParseKickoffUtc(string? utcTime)
    {
        if (string.IsNullOrWhiteSpace(utcTime))
            return null;

        // Không dùng RoundtripKind cùng AdjustToUniversal (ArgumentException trên .NET).
        if (DateTimeOffset.TryParse(
                utcTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dto))
            return dto.UtcDateTime;

        return null;
    }
}
