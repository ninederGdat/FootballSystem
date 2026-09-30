using System.ComponentModel.DataAnnotations;
using FootballApi.Common.Exceptions;
using FootballApi.Configuration;
using FootballApi.DTOs.Transfers;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Team;
using FootballApi.Repositories.Transfer;
using FootballApi.Services.Season;
using FootballApi.Services.Transfer;
using FootballSystem.Shared.Models.Clean;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FotmobSync.Tests.Transfers;

public class TransferStatisticsTests
{
    private static readonly SeasonDefinition FirstSeason = new(
        "2025-26", "2025/26", new DateTime(2025, 8, 1), new DateTime(2026, 8, 1));
    private static readonly SeasonDefinition SecondSeason = new(
        "2026-27", "2026/27", new DateTime(2026, 8, 1), new DateTime(2027, 8, 1));

    [Fact]
    public async Task MissingTeamAndSeason_UseDefaultsAndCurrentConfiguredSeason()
    {
        var repository = new InMemoryTransferRepository([]);
        var teamRepository = new InMemoryTeamRepository(new TeamClean { TeamId = 8455, Name = "Chelsea FC" });
        var service = CreateService(repository, teamRepository, [
            new SeasonDefinition("current", "Current", new DateTime(2000, 1, 1), new DateTime(2100, 1, 1))
        ]);

        var result = await service.GetTransferStatisticsAsync(new TransferStatisticsQuery(), default);

        Assert.Equal(8455, result.TeamId);
        Assert.Equal("current", result.Season);
        Assert.Equal(8455, teamRepository.LastTeamId);
        Assert.Equal(8455, repository.LastTeamId);
    }

    [Fact]
    public async Task UnknownSeason_UsesBadRequestMiddlewareResponse()
    {
        var service = CreateService(new InMemoryTransferRepository([]), new InMemoryTeamRepository(null), [FirstSeason]);
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.GetTransferStatisticsAsync(new TransferStatisticsQuery { Season = "unknown" }, default));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task UnknownTeam_ReturnsEmptyNameAndZeroStatistics()
    {
        var repository = new InMemoryTransferRepository([Transfer(1, 8455, 20, false, 500m)]);
        var service = CreateService(repository, new InMemoryTeamRepository(null), [FirstSeason]);

        var result = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = FirstSeason.Code }, default);

        Assert.Equal(string.Empty, result.TeamName);
        AssertAllNumbersAreZero(result);
        Assert.Null(repository.LastTeamId);
    }

    [Fact]
    public async Task AggregatesPermanentAndLoanTransfers_AndAppliesLoanFilter()
    {
        var generatedLoanOut = Transfer(5, 8455, 14, true, 1_000_000m);
        generatedLoanOut.IsSystemGenerated = true;
        var repository = new InMemoryTransferRepository([
            Transfer(1, 10, 8455, false, null),
            Transfer(2, 11, 8455, false, 10m),
            Transfer(3, 8455, 12, false, 4m),
            Transfer(4, 13, 8455, true, 2_000_000m),
            generatedLoanOut
        ]);
        var service = CreateService(repository, Chelsea(), [FirstSeason]);

        var all = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = FirstSeason.Code, TeamId = 8455 }, default);
        var loans = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = FirstSeason.Code, OnLoan = true }, default);
        var permanent = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = FirstSeason.Code, OnLoan = false }, default);

        Assert.Equal(10m, all.TotalFeeToBuy);
        Assert.Equal(4m, all.TotalFeeToSell);
        Assert.Equal(2, all.PermanentBuyCount);
        Assert.Equal(1, all.PermanentSellCount);
        Assert.Equal(1, all.LoanInCount);
        Assert.Equal(1, all.LoanOutCount);
        Assert.Equal(all.TotalFeeToBuy - all.TotalFeeToSell, all.NetSpend);

        Assert.Equal(0m, loans.TotalFeeToBuy);
        Assert.Equal(0m, loans.TotalFeeToSell);
        Assert.Equal(0, loans.PermanentBuyCount);
        Assert.Equal(1, loans.LoanInCount);
        Assert.Equal(1, loans.LoanOutCount);

        Assert.Equal(10m, permanent.TotalFeeToBuy);
        Assert.Equal(4m, permanent.TotalFeeToSell);
        Assert.Equal(2, permanent.PermanentBuyCount);
        Assert.Equal(1, permanent.PermanentSellCount);
        Assert.Equal(0, permanent.LoanInCount);
        Assert.Equal(0, permanent.LoanOutCount);
    }

    [Fact]
    public async Task ConfiguredSeasonWithoutTransfers_ReturnsZeroStatistics()
    {
        var service = CreateService(new InMemoryTransferRepository([]), Chelsea(), [FirstSeason]);

        var result = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = FirstSeason.Code }, default);

        AssertAllNumbersAreZero(result);
    }

    [Fact]
    public async Task TransferAtExclusiveEnd_IsOnlyIncludedInNextSeason()
    {
        var boundaryTransfer = Transfer(1, 10, 8455, false, 3m);
        boundaryTransfer.TransferDate = SecondSeason.StartDate;
        var service = CreateService(
            new InMemoryTransferRepository([boundaryTransfer]),
            Chelsea(),
            [FirstSeason, SecondSeason]);

        var first = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = FirstSeason.Code }, default);
        var second = await service.GetTransferStatisticsAsync(
            new TransferStatisticsQuery { Season = SecondSeason.Code }, default);

        Assert.Equal(0, first.PermanentBuyCount);
        Assert.Equal(1, second.PermanentBuyCount);
        Assert.Equal(3m, second.TotalFeeToBuy);
    }

    [Fact]
    public void TeamIdMustBePositive()
    {
        var query = new TransferStatisticsQuery { TeamId = 0 };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            query,
            new ValidationContext(query),
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
    }

    private static TransferService CreateService(
        InMemoryTransferRepository repository,
        InMemoryTeamRepository teamRepository,
        IReadOnlyList<SeasonDefinition> seasons)
    {
        var seasonService = new SeasonService(Microsoft.Extensions.Options.Options.Create(
            new SeasonOptions { Seasons = seasons.ToList() }));

        return new TransferService(
            repository,
            new StubPlayerRepository(),
            NullLogger<TransferService>.Instance,
            seasonService,
            teamRepository);
    }

    private static InMemoryTeamRepository Chelsea() =>
        new(new TeamClean { TeamId = 8455, Name = "Chelsea FC" });

    private static TransferClean Transfer(
        long id,
        long fromClubId,
        long toClubId,
        bool onLoan,
        decimal? fee) => new()
        {
            Id = id,
            FromClubId = fromClubId,
            ToClubId = toClubId,
            OnLoan = onLoan,
            FeeValue = fee,
            TransferDate = new DateTime(2026, 1, 1)
        };

    private static void AssertAllNumbersAreZero(TransferStatisticsResponse result)
    {
        Assert.Equal(0m, result.TotalFeeToBuy);
        Assert.Equal(0m, result.TotalFeeToSell);
        Assert.Equal(0m, result.NetSpend);
        Assert.Equal(0, result.PermanentBuyCount);
        Assert.Equal(0, result.PermanentSellCount);
        Assert.Equal(0, result.LoanInCount);
        Assert.Equal(0, result.LoanOutCount);
    }

    private sealed class InMemoryTransferRepository(IEnumerable<TransferClean> transfers) : ITransferRepository
    {
        private readonly List<TransferClean> _transfers = transfers.ToList();

        public long? LastTeamId { get; private set; }

        public Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct = default) =>
            Task.FromResult(_transfers.Where(x => x.PlayerId == playerId).ToList());

        public Task<IReadOnlyList<TransferClean>> GetTransfersByDateRangeAsync(
            long teamId,
            DateTime from,
            DateTime toExclusive,
            bool? onLoan,
            CancellationToken ct = default)
        {
            LastTeamId = teamId;
            IReadOnlyList<TransferClean> result = _transfers
                .Where(x => x.FromClubId == teamId || x.ToClubId == teamId)
                .Where(x => x.TransferDate >= from && x.TransferDate < toExclusive)
                .Where(x => onLoan is null || x.OnLoan == onLoan)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<(IReadOnlyList<TransferClean> Items, int TotalCount)> SearchAsync(
            string playerName,
            long? playerId,
            long? fromClubId,
            long? toClubId,
            bool? onLoan,
            bool? contractExtension,
            string transferType,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize,
            CancellationToken ct) => throw new NotImplementedException();
    }

    private sealed class InMemoryTeamRepository(TeamClean? team) : ITeamRepository
    {
        public long? LastTeamId { get; private set; }

        public Task<TeamClean?> GetByIdAsync(long teamId, CancellationToken ct)
        {
            LastTeamId = teamId;
            return Task.FromResult(team?.TeamId == teamId ? team : null);
        }

        public Task<List<TeamClean>> GetByIdsAsync(List<long> teamIds, CancellationToken ct = default)
        {
            var teams = team is not null && teamIds.Contains(team.TeamId)
                ? new List<TeamClean> { team }
                : new List<TeamClean>();
            return Task.FromResult(teams);
        }
    }

    private sealed class StubPlayerRepository : IPlayerRepository
    {
        public Task<PlayerClean?> GetByIdAsync(long playerId, CancellationToken ct) => Task.FromResult<PlayerClean?>(null);
        public Task<List<PlayerClean>> GetByIdsAsync(IEnumerable<long> playerIds, CancellationToken ct) => Task.FromResult(new List<PlayerClean>());
        public Task<(List<PlayerClean> Items, int TotalCount)> SearchAsync(
            string? search,
            long? teamId,
            string? positionCode,
            string? nationality,
            string? transferStatus,
            int page,
            int pageSize,
            CancellationToken ct) => Task.FromResult((new List<PlayerClean>(), 0));
    }
}