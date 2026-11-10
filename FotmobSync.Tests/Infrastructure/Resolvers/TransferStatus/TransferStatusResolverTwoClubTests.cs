using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using Xunit;

namespace FotmobSync.Tests.Infrastructure.Resolvers.TransferStatus;

public class TransferStatusResolverTwoClubTests
{
    private const long Chelsea = 10;
    private const long Strasbourg = 11;
    private const long Other = 20;

    private static readonly DateTime Now = new(2026, 1, 15);
    private static readonly IReadOnlySet<long> Tracked = new HashSet<long> { Chelsea, Strasbourg };

    [Theory]
    [InlineData(Chelsea, Strasbourg)]
    [InlineData(Strasbourg, Chelsea)]
    public void Resolve_PermanentTransferBetweenTrackedClubs_IsCurrentAtDestination(long from, long to)
    {
        var status = Assert.Single(Resolve(Transfer(from, to)));

        Assert.Equal("current", status.status);
        Assert.Equal(to, status.ParentClubId);
        Assert.Equal(to, status.CurrentClubId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resolve_RoundTripBetweenTrackedClubs_UsesLatestTransfer(bool reverseOrder)
    {
        var transfers = new[]
        {
            Transfer(Chelsea, Strasbourg, id: 1, daysAgo: 60),
            Transfer(Strasbourg, Chelsea, id: 2, daysAgo: 10)
        };
        if (reverseOrder) Array.Reverse(transfers);

        var status = Assert.Single(Resolve(transfers));

        Assert.Equal("current", status.status);
        Assert.Equal(Chelsea, status.ParentClubId);
        Assert.Equal(Chelsea, status.CurrentClubId);
    }

    [Theory]
    [InlineData(Chelsea, Strasbourg)]
    [InlineData(Strasbourg, Chelsea)]
    public void Resolve_ActiveLoanBetweenTrackedClubs_IsLoanedFromParent(long from, long to)
    {
        var status = Assert.Single(Resolve(
            Transfer(from, to, onLoan: true, periodEnd: Now.AddDays(30))));

        Assert.Equal("loaned", status.status);
        Assert.Equal(from, status.ParentClubId);
        Assert.Equal(to, status.CurrentClubId);
    }

    [Theory]
    [InlineData(Chelsea, Strasbourg)]
    [InlineData(Strasbourg, Chelsea)]
    public void Resolve_ExpiredLoanBetweenTrackedClubs_ReturnsToParent(long from, long to)
    {
        var status = Assert.Single(Resolve(
            Transfer(from, to, onLoan: true, periodEnd: Now.AddDays(-1))));

        Assert.Equal("current", status.status);
        Assert.Equal(from, status.ParentClubId);
        Assert.Equal(from, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_BoughtByStrasbourgThenLoanedOut_IsLoanedFromStrasbourg()
    {
        var status = Assert.Single(Resolve(
            Transfer(Chelsea, Strasbourg, id: 1, daysAgo: 90),
            Transfer(Strasbourg, Other, id: 2, daysAgo: 10, onLoan: true, periodEnd: Now.AddDays(100))));

        Assert.Equal("loaned", status.status);
        Assert.Equal(Strasbourg, status.ParentClubId);
        Assert.Equal(Other, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_BoughtByStrasbourgThenSold_IsTransferredWithStrasbourgAsLastParent()
    {
        var status = Assert.Single(Resolve(
            Transfer(Chelsea, Strasbourg, id: 1, daysAgo: 90),
            Transfer(Strasbourg, Other, id: 2, daysAgo: 10)));

        Assert.Equal("transferred", status.status);
        Assert.Equal(Strasbourg, status.ParentClubId);
        Assert.Equal(Other, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_ContractExtensionAfterInternalTransfer_DoesNotChangeStatus()
    {
        var extension = Transfer(Strasbourg, Strasbourg, id: 2, daysAgo: 5);
        extension.ContractExtension = true;

        var status = Assert.Single(Resolve(
            Transfer(Chelsea, Strasbourg, id: 1, daysAgo: 60),
            extension));

        Assert.Equal("current", status.status);
        Assert.Equal(Strasbourg, status.ParentClubId);
        Assert.Equal(Strasbourg, status.CurrentClubId);
    }

    private static IReadOnlyList<PlayerTransferStatus> Resolve(params TransferClean[] transfers) =>
        new TransferStatusResolver().Resolve(transfers, Now, Tracked);

    private static TransferClean Transfer(
        long from,
        long to,
        bool onLoan = false,
        DateTime? periodEnd = null,
        long id = 1,
        int daysAgo = 30) =>
        new()
        {
            Id = id,
            PlayerId = 100,
            PlayerName = "Test Player",
            FromClubId = from,
            ToClubId = to,
            TransferDate = Now.AddDays(-daysAgo),
            OnLoan = onLoan,
            PeriodEnd = periodEnd
        };
}