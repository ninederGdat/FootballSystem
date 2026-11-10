using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using Xunit;

namespace FotmobSync.Tests.Infrastructure.Resolvers.TransferStatus;

public class TransferStatusResolverTests
{
    private static readonly DateTime CurrentDate = new(2026, 1, 15);
    private static readonly IReadOnlySet<long> TrackedTeamIds = new HashSet<long> { 10 };

    [Fact]
    public void Resolve_PermanentMoveToTrackedClub_ReturnsCurrent()
    {
        var result = Resolve(CreateTransfer(fromClubId: 20, toClubId: 10));

        var status = Assert.Single(result);
        Assert.Equal("current", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(10, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_ActiveLoanFromUntrackedClub_ReturnsCurrent()
    {
        var result = Resolve(CreateTransfer(
            fromClubId: 20,
            toClubId: 10,
            onLoan: true,
            periodEnd: CurrentDate.AddDays(1)));

        var status = Assert.Single(result);
        Assert.Equal("current", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(10, status.CurrentClubId);
        // REVIEW: The resolver currently represents an active loan into a tracked club as current.
    }

    [Fact]
    public void Resolve_ActiveLoanFromTrackedClub_ReturnsLoanedOut()
    {
        var result = Resolve(CreateTransfer(
            fromClubId: 10,
            toClubId: 20,
            onLoan: true,
            periodEnd: CurrentDate.AddDays(1)));

        var status = Assert.Single(result);
        Assert.Equal("loaned", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(20, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_PermanentMoveAwayFromTrackedClub_ReturnsTransferred()
    {
        var result = Resolve(CreateTransfer(fromClubId: 10, toClubId: 20));

        var status = Assert.Single(result);
        Assert.Equal("transferred", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(20, status.CurrentClubId);
    }

    [Theory]
    [InlineData(10, 20, 10, 10)]
    [InlineData(20, 10, 10, 10)]
    public void Resolve_ExpiredLoan_ReturnsCurrentAtParentClub(
        long fromClubId,
        long toClubId,
        long expectedParentClubId,
        long expectedCurrentClubId)
    {
        var result = Resolve(CreateTransfer(
            fromClubId: fromClubId,
            toClubId: toClubId,
            onLoan: true,
            periodEnd: CurrentDate.AddDays(-1)));

        var status = Assert.Single(result);
        Assert.Equal("current", status.status);
        Assert.Equal(expectedParentClubId, status.ParentClubId);
        Assert.Equal(expectedCurrentClubId, status.CurrentClubId);
        // REVIEW: An expired loan resolves to current at its parent club; another policy could retain a loan status.
    }

    [Theory]
    [InlineData(-1, "current", 10, 10)]
    [InlineData(0, "current", 10, 10)]
    [InlineData(1, "loaned", 10, 20)]
    public void Resolve_LoanPeriodEndBoundary_UsesStrictlyFutureEndDate(
        int periodEndOffsetDays,
        string expectedStatus,
        long expectedParentClubId,
        long expectedCurrentClubId)
    {
        var result = Resolve(CreateTransfer(
            fromClubId: 10,
            toClubId: 20,
            onLoan: true,
            periodEnd: CurrentDate.AddDays(periodEndOffsetDays)));

        var status = Assert.Single(result);
        Assert.Equal(expectedStatus, status.status);
        Assert.Equal(expectedParentClubId, status.ParentClubId);
        Assert.Equal(expectedCurrentClubId, status.CurrentClubId);
    }

    [Theory]
    [InlineData(30, "loaned", 10, 20)]
    [InlineData(364, "loaned", 10, 20)]
    [InlineData(365, "current", 10, 10)]   // đúng 1 năm: không còn hiệu lực (so sánh lớn hơn nghiêm ngặt)
    public void Resolve_LoanWithoutPeriodEnd_IsActiveForUpToOneYear(
       int transferDaysAgo,
       string expectedStatus,
       long expectedParentClubId,
       long expectedCurrentClubId)
    {
        var result = Resolve(CreateTransfer(
            fromClubId: 10,
            toClubId: 20,
            onLoan: true,
            periodEnd: null,
            transferDateDaysAgo: transferDaysAgo));

        var status = Assert.Single(result);
        Assert.Equal(expectedStatus, status.status);
        Assert.Equal(expectedParentClubId, status.ParentClubId);
        Assert.Equal(expectedCurrentClubId, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_LoanWithoutPeriodEnd_CountsFromPeriodStartWhenPresent()
    {
        var transfer = CreateTransfer(
            fromClubId: 10,
            toClubId: 20,
            onLoan: true,
            periodEnd: null,
            transferDateDaysAgo: 400);
        transfer.PeriodStart = CurrentDate.AddDays(-30);

        var status = Assert.Single(Resolve(transfer));

        Assert.Equal("loaned", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(20, status.CurrentClubId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resolve_ReturnToTrackedClub_UsesLatestTransferRegardlessOfInputOrder(bool reverseOrder)
    {
        var transfers = new[]
        {
            CreateTransfer(fromClubId: 10, toClubId: 20, transferDateDaysAgo: 20),
            CreateTransfer(fromClubId: 20, toClubId: 10, id: 2, transferDateDaysAgo: 10)
        };
        var input = reverseOrder ? transfers.Reverse() : transfers;

        var result = Resolve(input.ToArray());

        var status = Assert.Single(result);
        Assert.Equal("current", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(10, status.CurrentClubId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resolve_LoanThenPermanentSale_UsesLatestTransferRegardlessOfInputOrder(bool reverseOrder)
    {
        var transfers = new[]
        {
            CreateTransfer(
                fromClubId: 10,
                toClubId: 20,
                onLoan: true,
                periodEnd: CurrentDate.AddDays(10),
                transferDateDaysAgo: 20),
            CreateTransfer(fromClubId: 10, toClubId: 30, id: 2, transferDateDaysAgo: 10)
        };
        var input = reverseOrder ? transfers.Reverse() : transfers;

        var result = Resolve(input.ToArray());

        var status = Assert.Single(result);
        Assert.Equal("transferred", status.status);
        Assert.Equal(10, status.ParentClubId);
        Assert.Equal(30, status.CurrentClubId);
    }

    [Fact]
    public void Resolve_TransferBetweenUntrackedClubs_IsSkipped()
    {
        var result = Resolve(CreateTransfer(fromClubId: 20, toClubId: 30));

        Assert.Empty(result);
    }

    [Fact]
    public void Resolve_MultiplePlayers_PreservesFirstSeenGroupOrder()
    {
        var result = Resolve(
            CreateTransfer(fromClubId: 10, toClubId: 20, playerId: 202),
            CreateTransfer(fromClubId: 30, toClubId: 10, playerId: 101, id: 2));

        Assert.Collection(
            result,
            status =>
            {
                Assert.Equal(202, status.PlayerId);
                Assert.Equal("transferred", status.status);
                Assert.Equal(10, status.ParentClubId);
                Assert.Equal(20, status.CurrentClubId);
            },
            status =>
            {
                Assert.Equal(101, status.PlayerId);
                Assert.Equal("current", status.status);
                Assert.Equal(10, status.ParentClubId);
                Assert.Equal(10, status.CurrentClubId);
            });
    }

    private static IReadOnlyList<PlayerTransferStatus> Resolve(params TransferClean[] transfers) =>
        new TransferStatusResolver().Resolve(transfers, CurrentDate, TrackedTeamIds);

    private static TransferClean CreateTransfer(
        long fromClubId,
        long toClubId,
        bool onLoan = false,
        DateTime? periodEnd = null,
        long playerId = 100,
        long id = 1,
        int transferDateDaysAgo = 30) =>
        new()
        {
            Id = id,
            PlayerId = playerId,
            PlayerName = "Test Player",
            FromClubId = fromClubId,
            ToClubId = toClubId,
            TransferDate = CurrentDate.AddDays(-transferDateDaysAgo),
            OnLoan = onLoan,
            PeriodEnd = periodEnd
        };
}
