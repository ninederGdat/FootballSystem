using FootballApi.DTOs.Lineups;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.DTOs.Matches;

public record MatchDetailResponse(
    long MatchId,
    long? TeamId,
    long? OpponentTeamId,
    string OpponentName,
    long? CompetitionId,
    string? CompetitionName,
    DateTime MatchDate,
    string? HomeOrAway,
    int? ScoreHome,
    int? ScoreAway,
    string Status,
    List<MatchEventResponse> Events,
    LineupResponse? Lineup)
{
    public static MatchDetailResponse FromClean(
        MatchClean clean,
        List<MatchEventResponse> events,
        LineupResponse? lineup) => new(
        clean.MatchId,
        clean.TeamId,
        clean.OpponentTeamId,
        clean.OpponentName,
        clean.CompetitionId,
        clean.CompetitionName,
        clean.MatchDate,
        clean.HomeOrAway,
        clean.ScoreHome,
        clean.ScoreAway,
        clean.Status,
        events,
        lineup);
}