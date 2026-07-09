using FootballSystem.Shared.Models.Clean;

namespace FootballApi.DTOs.Responses;

public record MatchResponse(
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
    string Status)
{
    public static MatchResponse FromClean(MatchClean clean) => new(
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
        clean.Status);
}