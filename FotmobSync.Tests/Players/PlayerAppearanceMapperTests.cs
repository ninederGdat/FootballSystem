using FootballApi.Repositories.Lineup;
using Xunit;

namespace FotmobSync.Tests.Players;

public class PlayerAppearanceMapperTests
{
    private static PlayerAppearanceViewClean Row(
        bool? isStarter = true, long? roleId = null) => new()
        {
            LineupPlayerId = 10,
            PlayerId = 1,
            LineupId = 5,
            MatchId = 99,
            MatchDate = new DateTime(2026, 11, 8, 15, 0, 0, DateTimeKind.Utc),
            OpponentName = "Liverpool",
            CompetitionName = "Carabao Cup",
            PlayedForTeamId = 8455,
            IsStarter = isStarter,
            PositionCode = "CM",
            RoleId = roleId,
            MinuteIn = 0,
            MinuteOut = 78
        };

    [Fact]
    public void ToRecord_CopiesAllFieldsAndGoalAssistCounts()
    {
        var record = Row(roleId: 7).ToRecord(goals: 2, assists: 1);

        Assert.Equal(99, record.MatchId);
        Assert.Equal(new DateTime(2026, 11, 8, 15, 0, 0, DateTimeKind.Utc), record.MatchDate);
        Assert.Equal("Liverpool", record.OpponentName);
        Assert.Equal("Carabao Cup", record.CompetitionName);
        Assert.Equal(8455, record.PlayedForTeamId);
        Assert.True(record.IsStarter);
        Assert.Equal("CM", record.PositionCode);
        Assert.Equal(7, record.RoleId);
        Assert.Equal(0, record.MinuteIn);
        Assert.Equal(78, record.MinuteOut);
        Assert.Equal(2, record.Goals);
        Assert.Equal(1, record.Assists);
    }

    [Fact]
    public void ToRecord_NullIsStarter_DefaultsToTrue()
    {
        Assert.True(Row(isStarter: null).ToRecord(0, 0).IsStarter);
    }

    [Fact]
    public void ToRecord_NullRoleId_StaysNull()
    {
        Assert.Null(Row(roleId: null).ToRecord(0, 0).RoleId);
    }

    [Fact]
    public void ToRecord_RoleIdOutsideIntRange_Throws()
    {
        Assert.Throws<OverflowException>(() => Row(roleId: long.MaxValue).ToRecord(0, 0));
    }
}