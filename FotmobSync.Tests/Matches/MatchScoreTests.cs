using FootballApi.DTOs.Matches;
using FootballSystem.Shared.Models.Clean;
using Xunit;

namespace FotmobSync.Tests.Matches;

public class MatchScoreTests
{
    [Fact]
    public void RegularScore_ShootoutMatch_SubtractsPenaltyScoreFromCumulativeScore()
    {
        var match = CreateMatch(scoreHome: 7, scoreAway: 6, penaltyHome: 6, penaltyAway: 5);

        Assert.Equal(1, match.RegularScoreHome());
        Assert.Equal(1, match.RegularScoreAway());
    }

    [Fact]
    public void RegularScore_NoShootout_ReturnsScoreUnchanged()
    {
        var match = CreateMatch(scoreHome: 3, scoreAway: 1);

        Assert.Equal(3, match.RegularScoreHome());
        Assert.Equal(1, match.RegularScoreAway());
    }

    [Fact]
    public void RegularScore_UnknownScore_ReturnsNull()
    {
        var match = CreateMatch(scoreHome: null, scoreAway: null);

        Assert.Null(match.RegularScoreHome());
        Assert.Null(match.RegularScoreAway());
    }

    [Fact]
    public void FromClean_ExposesRegularScoreAndPenaltyScore()
    {
        var match = CreateMatch(scoreHome: 7, scoreAway: 6, penaltyHome: 6, penaltyAway: 5);

        var detail = MatchDetailResponse.FromClean(match, new List<MatchEventResponse>(), null);

        Assert.Equal(1, detail.ScoreHome);
        Assert.Equal(1, detail.ScoreAway);
        Assert.Equal(6, detail.PenaltyHome);
        Assert.Equal(5, detail.PenaltyAway);
    }

    private static MatchClean CreateMatch(
        int? scoreHome,
        int? scoreAway,
        int? penaltyHome = null,
        int? penaltyAway = null) =>
        new()
        {
            MatchId = 1001,
            TeamId = 8455,
            OpponentName = "Liverpool",
            MatchDate = new DateTime(2026, 10, 28, 19, 45, 0, DateTimeKind.Utc),
            HomeOrAway = "away",
            Status = "FINISHED",
            ScoreHome = scoreHome,
            ScoreAway = scoreAway,
            PenaltyHome = penaltyHome,
            PenaltyAway = penaltyAway
        };
}