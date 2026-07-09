using System.Globalization;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Models.Raw;

namespace FotmobSync.Mappers;

/// <summary>
/// Mapper từ <see cref="MatchRaw"/> sang <see cref="MatchClean"/> theo góc nhìn <paramref name="teamId"/>.
/// </summary>
public static class MatchMapper
{
    public static MatchClean? ToClean(this MatchRaw? raw, long teamId)
    {
        if (raw == null || raw.Id == 0 || raw.Home == null || raw.Away == null)
            return null;

        var matchDate = ParseMatchDate(raw.Status?.UtcTime);
        if (matchDate == null)
            return null;

        string? homeOrAway;
        long opponentTeamId;
        string opponentName;

        if (raw.Home.Id == teamId)
        {
            homeOrAway = "home";
            opponentTeamId = raw.Away.Id;
            opponentName = raw.Away.Name?.Trim() ?? string.Empty;
        }
        else if (raw.Away.Id == teamId)
        {
            homeOrAway = "away";
            opponentTeamId = raw.Home.Id;
            opponentName = raw.Home.Name?.Trim() ?? string.Empty;
        }
        else
            return null;

        if (string.IsNullOrEmpty(opponentName))
            opponentName = "Unknown";

        var leagueId = raw.Tournament?.LeagueId;
        return new MatchClean
        {
            MatchId = raw.Id,
            TeamId = teamId,
            OpponentTeamId = opponentTeamId,
            OpponentName = opponentName,
            CompetitionId = leagueId is > 0 ? leagueId : null,
            CompetitionName = raw.Tournament?.Name?.Trim(),
            MatchDate = matchDate.Value,
            HomeOrAway = homeOrAway,
            ScoreHome = raw.Home.Score,
            ScoreAway = raw.Away.Score,
            Status = MapStatus(raw.Status),
            LastUpdated = DateTime.UtcNow
        };
    }

    public static List<MatchClean> ToCleanList(this IEnumerable<MatchRaw?> rawList, long teamId) =>
        rawList.Select(r => r.ToClean(teamId)).Where(c => c != null).Select(c => c!).ToList();

    /// <summary>UPCOMING | ONGOING | FINISHED (theo .cursorrules).</summary>
    private static string MapStatus(MatchStatusRaw? status)
    {
        if (status == null)
            return "UPCOMING";

        if (status.Cancelled || status.Finished)
            return "FINISHED";

        if (status.Started)
            return "ONGOING";

        return "UPCOMING";
    }

    private static DateTime? ParseMatchDate(string? utcTime)
    {
        if (string.IsNullOrWhiteSpace(utcTime))
            return null;

        if (DateTimeOffset.TryParse(
                utcTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dto))
            return dto.UtcDateTime;

        return null;
    }
}
