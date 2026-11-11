using System.Reflection;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Mappers;
using Supabase.Postgrest.Attributes;
using Xunit;

namespace FotmobSync.Tests.Mappers;

public class MatchUpsertTests
{
    [Fact]
    public void ToUpsert_CopiesEveryFixtureColumn()
    {
        var clean = new MatchClean
        {
            MatchId = 1001,
            TeamId = 8455,
            OpponentTeamId = 8650,
            OpponentName = "Liverpool",
            CompetitionId = 133,
            CompetitionName = "EFL Cup",
            MatchDate = new DateTime(2026, 10, 28, 19, 45, 0, DateTimeKind.Utc),
            HomeOrAway = "away",
            ScoreHome = 7,
            ScoreAway = 6,
            Status = "FINISHED",
            LastUpdated = new DateTime(2026, 11, 10, 2, 0, 0, DateTimeKind.Utc)
        };

        var upsert = clean.ToUpsert();

        Assert.Equal(clean.MatchId, upsert.MatchId);
        Assert.Equal(clean.TeamId, upsert.TeamId);
        Assert.Equal(clean.OpponentTeamId, upsert.OpponentTeamId);
        Assert.Equal(clean.OpponentName, upsert.OpponentName);
        Assert.Equal(clean.CompetitionId, upsert.CompetitionId);
        Assert.Equal(clean.CompetitionName, upsert.CompetitionName);
        Assert.Equal(clean.MatchDate, upsert.MatchDate);
        Assert.Equal(clean.HomeOrAway, upsert.HomeOrAway);
        Assert.Equal(clean.ScoreHome, upsert.ScoreHome);
        Assert.Equal(clean.ScoreAway, upsert.ScoreAway);
        Assert.Equal(clean.Status, upsert.Status);
        Assert.Equal(clean.LastUpdated, upsert.LastUpdated);
    }

    [Fact]
    public void MatchUpsert_DoesNotWritePenaltyColumns()
    {
        var columns = typeof(MatchUpsert)
            .GetProperties()
            .Select(p => p.GetCustomAttribute<ColumnAttribute>()?.ColumnName)
            .OfType<string>()
            .ToList();

        Assert.Contains("score_home", columns);
        Assert.DoesNotContain("penalty_home", columns);
        Assert.DoesNotContain("penalty_away", columns);
    }
}