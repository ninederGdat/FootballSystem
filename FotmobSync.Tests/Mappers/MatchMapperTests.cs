using FotmobSync.Mappers;
using FotmobSync.Models.Raw;
using Xunit;

namespace FotmobSync.Tests.Mappers;

public class MatchMapperTests
{
    private const long Chelsea = 8455;
    private const long Strasbourg = 9848;
    private const long Other = 8456;

    private static readonly HashSet<long> Tracked = new() { Chelsea, Strasbourg };

    private static MatchRaw CreateMatch(long homeId, long awayId) => new()
    {
        Id = 1001,
        Home = new MatchTeamSideRaw { Id = (int)homeId, Name = "Home" },
        Away = new MatchTeamSideRaw { Id = (int)awayId, Name = "Away" }
    };

    [Fact]
    public void IsOwnedBy_InternalMatch_HomeTeamOwns()
    {
        var match = CreateMatch(Chelsea, Strasbourg);

        Assert.True(match.IsOwnedBy(Chelsea, Tracked));
    }

    [Fact]
    public void IsOwnedBy_InternalMatch_AwayTeamDoesNotOwn()
    {
        var match = CreateMatch(Chelsea, Strasbourg);

        Assert.False(match.IsOwnedBy(Strasbourg, Tracked));
    }

    [Fact]
    public void IsOwnedBy_InternalMatch_OwnerFlipsWhenHomeAndAwayAreSwapped()
    {
        var match = CreateMatch(Strasbourg, Chelsea);

        Assert.True(match.IsOwnedBy(Strasbourg, Tracked));
        Assert.False(match.IsOwnedBy(Chelsea, Tracked));
    }

    [Fact]
    public void IsOwnedBy_InternalMatch_ExactlyOneTrackedTeamOwns()
    {
        var match = CreateMatch(Chelsea, Strasbourg);

        var owners = Tracked.Count(id => match.IsOwnedBy(id, Tracked));

        Assert.Equal(1, owners);
    }

    [Fact]
    public void IsOwnedBy_TrackedHomeVsUntrackedAway_HomeOwns()
    {
        var match = CreateMatch(Chelsea, Other);

        Assert.True(match.IsOwnedBy(Chelsea, Tracked));
    }

    [Fact]
    public void IsOwnedBy_TrackedAwayVsUntrackedHome_AwayOwns()
    {
        var match = CreateMatch(Other, Chelsea);

        Assert.True(match.IsOwnedBy(Chelsea, Tracked));
    }

    [Fact]
    public void IsOwnedBy_TeamNotInMatch_ReturnsFalse()
    {
        var match = CreateMatch(Other, 9999);

        Assert.False(match.IsOwnedBy(Chelsea, Tracked));
    }

    [Fact]
    public void IsOwnedBy_MissingSide_ReturnsFalse()
    {
        Assert.False(new MatchRaw { Id = 1, Home = null, Away = null }.IsOwnedBy(Chelsea, Tracked));
        Assert.False(((MatchRaw?)null).IsOwnedBy(Chelsea, Tracked));
    }

    [Fact]
    public void IsOwnedBy_OnlyOneTrackedTeamConfigured_AwayTeamOwnsItsMatches()
    {
        var onlyChelsea = new HashSet<long> { Chelsea };
        var match = CreateMatch(Strasbourg, Chelsea);

        Assert.True(match.IsOwnedBy(Chelsea, onlyChelsea));
    }
}