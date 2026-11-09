using FootballApi.Common.Exceptions;
using FootballApi.Configuration;
using FootballApi.Controllers;
using FootballApi.DTOs.Common;
using FootballApi.DTOs.Lineups;
using FootballApi.DTOs.Matches;
using FootballApi.Repositories.Match;
using FootballApi.Repositories.Team;
using FootballApi.Services;
using FootballApi.Services.MatchEvent;
using FootballApi.Services.Season;
using FootballSystem.Shared.Models.Clean;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

namespace FotmobSync.Tests.Matches;

public class MatchSearchApiTests
{
    private static readonly DateTime SeasonStart = new(2025, 8, 1);
    private static readonly DateTime SeasonEnd = new(2026, 8, 1);

    public static TheoryData<string> MatchFilters => new()
    {
        "fromDate", "toDate", "season", "opponent", "teamId", "competitionId", "status"
    };

    [Theory]
    [MemberData(nameof(MatchFilters))]
    public async Task SearchMatchesAsync_FilterIsForwarded(string filter)
    {
        var repository = new RecordingMatchRepository();
        var service = CreateService(repository);
        var query = new MatchSearchQuery();

        switch (filter)
        {
            case "fromDate":
                query.FromDate = new DateTime(2025, 9, 1);
                break;
            case "toDate":
                query.ToDate = new DateTime(2026, 7, 1);
                break;
            case "season":
                query.Season = "2025-26";
                break;
            case "opponent":
                query.Opponent = "Arsenal";
                break;
            case "teamId":
                query.TeamId = 8455;
                break;
            case "competitionId":
                query.CompetitionId = 42;
                break;
            case "status":
                query.Status = "FINISHED";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
        }

        await service.SearchMatchesAsync(query);

        Assert.Equal(query.Opponent, repository.Opponent);
        Assert.Equal(query.Status, repository.Status);
        Assert.Equal(query.TeamId, repository.TeamId);
        Assert.Equal(query.CompetitionId, repository.CompetitionId);
        if (filter == "fromDate")
            Assert.Equal(query.FromDate, repository.FromDate);
        if (filter == "toDate")
            Assert.Equal(query.ToDate, repository.ToDate);
        if (filter == "season")
        {
            Assert.Equal(SeasonStart, repository.FromDate);
            Assert.Equal(SeasonEnd, repository.ToDate);
        }
    }

    [Theory]
    [MemberData(nameof(MatchFilters))]
    public async Task SearchMatchesAsync_FilterWithoutValue_PassesNull(string filter)
    {
        var repository = new RecordingMatchRepository();
        await CreateService(repository).SearchMatchesAsync(new MatchSearchQuery());

        switch (filter)
        {
            case "fromDate":
                Assert.Null(repository.FromDate);
                break;
            case "toDate":
            case "season":
                Assert.Null(repository.ToDate);
                break;
            case "opponent":
                Assert.Null(repository.Opponent);
                break;
            case "teamId":
                Assert.Null(repository.TeamId);
                break;
            case "competitionId":
                Assert.Null(repository.CompetitionId);
                break;
            case "status":
                Assert.Null(repository.Status);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
        }
    }

    [Fact]
    public async Task SearchMatchesAsync_NoFilters_PassesNullValues()
    {
        var repository = new RecordingMatchRepository();
        var service = CreateService(repository);

        await service.SearchMatchesAsync(new MatchSearchQuery());

        Assert.Null(repository.FromDate);
        Assert.Null(repository.ToDate);
        Assert.Null(repository.Opponent);
        Assert.Null(repository.Status);
        Assert.Null(repository.TeamId);
        Assert.Null(repository.CompetitionId);
        Assert.False(repository.Descending);
    }

    [Fact]
    public async Task SearchMatchesAsync_CombinedFiltersWithNoMatches_ReturnsEmptyPage()
    {
        var repository = new RecordingMatchRepository { TotalCount = 0 };
        var service = CreateService(repository);
        var query = new MatchSearchQuery
        {
            FromDate = new DateTime(2025, 9, 1),
            ToDate = new DateTime(2026, 7, 1),
            Season = "2025-26",
            Opponent = "Nonexistent",
            TeamId = 8455,
            CompetitionId = 42,
            Status = "FINISHED",
            Sort = "desc",
            Page = 2,
            PageSize = 3
        };

        var result = await service.SearchMatchesAsync(query);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
        Assert.Equal(query.Opponent, repository.Opponent);
        Assert.Equal(query.Status, repository.Status);
        Assert.Equal(query.TeamId, repository.TeamId);
        Assert.Equal(query.CompetitionId, repository.CompetitionId);
        Assert.True(repository.Descending);
        Assert.Equal(new DateTime(2025, 9, 1), repository.FromDate);
        Assert.Equal(new DateTime(2026, 7, 1), repository.ToDate);
    }

    [Fact]
    public async Task SearchMatchesAsync_UnknownSeason_ThrowsBadRequest()
    {
        var service = CreateService(new RecordingMatchRepository());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.SearchMatchesAsync(new MatchSearchQuery { Season = "unknown" }));
    }

    [Fact]
    public async Task SearchMatchesAsync_UntrackedTeam_ThrowsBadRequest()
    {
        var service = CreateService(new RecordingMatchRepository());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.SearchMatchesAsync(new MatchSearchQuery { TeamId = 9999 }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchMatchesAsync_NonPositiveTeamId_ThrowsBadRequest(long teamId)
    {
        var service = CreateService(new RecordingMatchRepository());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.SearchMatchesAsync(new MatchSearchQuery { TeamId = teamId }));
    }

    [Theory]
    [InlineData("descending")]
    [InlineData("random")]
    public async Task SearchMatchesAsync_InvalidSort_ThrowsBadRequest(string sort)
    {
        var service = CreateService(new RecordingMatchRepository());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.SearchMatchesAsync(new MatchSearchQuery { Sort = sort }));
    }

    [Theory]
    [InlineData(1, 5, 11, 3)]
    [InlineData(3, 5, 11, 3)]
    [InlineData(4, 5, 11, 3)]
    [InlineData(1, 20, 11, 1)]
    [InlineData(1, 5, 0, 0)]
    public async Task SearchMatchesAsync_Page_ReturnsCorrectPaginationMetadata(
        int page, int pageSize, int totalItems, int totalPages)
    {
        var repository = new RecordingMatchRepository
        {
            Items = [Match(1), Match(2)],
            TotalCount = totalItems
        };
        var service = CreateService(repository);

        var result = await service.SearchMatchesAsync(new MatchSearchQuery
        {
            Page = page,
            PageSize = pageSize
        });

        Assert.Equal(page, result.Pagination.Page);
        Assert.Equal(pageSize, result.Pagination.PageSize);
        Assert.Equal(totalItems, result.Pagination.TotalItems);
        Assert.Equal(totalPages, result.Pagination.TotalPages);
    }

    [Theory]
    [InlineData(0, 0, 1, 5)]
    [InlineData(-2, 101, 1, 5)]
    [InlineData(3, 101, 3, 5)]
    [InlineData(3, 20, 3, 20)]
    public async Task SearchMatches_ControllerNormalizesPageParameters(
        int requestedPage, int requestedPageSize, int expectedPage, int expectedPageSize)
    {
        var service = new CapturingMatchService();
        var controller = new MatchesController(service);

        await controller.SearchMatches(
            fromDate: null,
            toDate: null,
            season: null,
            opponent: null,
            teamId: null,
            competitionId: null,
            status: null,
            sort: "asc",
            page: requestedPage,
            pageSize: requestedPageSize);

        Assert.Equal(expectedPage, service.Query!.Page);
        Assert.Equal(expectedPageSize, service.Query.PageSize);
    }

    [Theory]
    [InlineData("asc", false)]
    [InlineData("desc", true)]
    [InlineData(" DESC ", true)]
    public async Task SearchMatchesAsync_SortPassesExpectedDirection(string sort, bool descending)
    {
        var repository = new RecordingMatchRepository();
        var service = CreateService(repository);

        await service.SearchMatchesAsync(new MatchSearchQuery { Sort = sort });

        Assert.Equal(descending, repository.Descending);
    }

    [Fact]
    public async Task SearchMatchesAsync_DefaultSort_IsAscending()
    {
        var repository = new RecordingMatchRepository();

        await CreateService(repository).SearchMatchesAsync(new MatchSearchQuery());

        Assert.False(repository.Descending);
    }

    [Fact]
    public async Task SearchMatchesAsync_SeasonEnd_IsPassedAsExclusiveUpperBound()
    {
        var repository = new RecordingMatchRepository();
        var service = CreateService(repository);

        await service.SearchMatchesAsync(new MatchSearchQuery { Season = "2025-26" });

        Assert.Equal(SeasonEnd, repository.ToDate);
    }

    [Fact]
    public async Task SearchMatchesAsync_MissingTeamId_LeavesBothTrackedClubsInScope()
    {
        // REVIEW: Null intentionally includes both tracked clubs; using a configured default team would be an alternative.
        var repository = new RecordingMatchRepository
        {
            Items =
            [
                Match(1, teamId: 8455, opponentTeamId: 100),
                Match(2, teamId: 100, opponentTeamId: 8455)
            ],
            TotalCount = 2
        };

        var result = await CreateService(repository).SearchMatchesAsync(new MatchSearchQuery());

        Assert.Null(repository.TeamId);
        Assert.Equal(new long[] { 1, 2 }, result.Data.Select(match => match.MatchId));
    }

    [Fact]
    public async Task SearchMatchesAsync_EqualDateRows_PreservesRepositoryOrderAcrossPages()
    {
        var firstPage = new RecordingMatchRepository
        {
            ItemsByPage = new Dictionary<int, List<MatchClean>>
            {
                [1] = [Match(10), Match(20)]
            },
            TotalCount = 4
        };
        var secondPage = new RecordingMatchRepository
        {
            ItemsByPage = new Dictionary<int, List<MatchClean>>
            {
                [2] = [Match(30), Match(40)]
            },
            TotalCount = 4
        };
        var pageOne = await CreateService(firstPage).SearchMatchesAsync(new MatchSearchQuery { Page = 1, PageSize = 2 });
        var pageTwo = await CreateService(secondPage).SearchMatchesAsync(new MatchSearchQuery { Page = 2, PageSize = 2 });
        var ids = pageOne.Data.Concat(pageTwo.Data).Select(match => match.MatchId).ToList();

        Assert.Equal(new long[] { 10, 20, 30, 40 }, ids);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public async Task SearchMatchesAsync_NullableMatchFields_MapWithoutThrowing()
    {
        var repository = new RecordingMatchRepository
        {
            Items =
            [
                new MatchClean
                {
                    MatchId = 1,
                    MatchDate = new DateTime(2026, 1, 1),
                    OpponentName = "Opponent",
                    CompetitionName = null,
                    HomeOrAway = null,
                    ScoreHome = null,
                    ScoreAway = null
                }
            ],
            TotalCount = 1
        };

        var result = await CreateService(repository).SearchMatchesAsync(new MatchSearchQuery());
        var item = Assert.Single(result.Data);

        Assert.Null(item.CompetitionName);
        Assert.Null(item.HomeOrAway);
        Assert.Null(item.ScoreHome);
        Assert.Null(item.ScoreAway);
    }

    private static MatchService CreateService(RecordingMatchRepository repository) =>
        new(
            repository,
            new StubMatchEventService(),
            new StubLineupService(),
            new SeasonService(Microsoft.Extensions.Options.Options.Create(new SeasonOptions
            {
                Seasons =
                [
                    new SeasonDefinition("2025-26", "2025/26", SeasonStart, SeasonEnd)
                ]
            })),
            new FakeTeamRepository([new TeamClean { TeamId = 8455 }, new TeamClean { TeamId = 100 }]));

    private static MatchClean Match(long id, long? teamId = 8455, long? opponentTeamId = 100) => new()
    {
        MatchId = id,
        TeamId = teamId,
        OpponentTeamId = opponentTeamId,
        MatchDate = new DateTime(2026, 1, 1),
        OpponentName = $"Opponent {id}"
    };

    private sealed class RecordingMatchRepository : IMatchRepository
    {
        public DateTime? FromDate { get; private set; }
        public DateTime? ToDate { get; private set; }
        public string? Opponent { get; private set; }
        public string? Status { get; private set; }
        public long? TeamId { get; private set; }
        public long? CompetitionId { get; private set; }
        public bool Descending { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; init; }
        public List<MatchClean> Items { get; init; } = [];
        public Dictionary<int, List<MatchClean>> ItemsByPage { get; init; } = [];

        public Task<MatchClean?> GetByMatchIdAsync(long matchId, CancellationToken ct = default) =>
            Task.FromResult<MatchClean?>(null);

        public Task<List<MatchClean>> GetByMatchIdsAsync(IEnumerable<long> matchIds, CancellationToken ct = default) =>
            Task.FromResult(new List<MatchClean>());

        public Task<(List<MatchClean> Items, int TotalCount)> SearchAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? opponent,
            string? status,
            long? teamId,
            long? competitionId,
            bool descending,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            FromDate = fromDate;
            ToDate = toDate;
            Opponent = opponent;
            Status = status;
            TeamId = teamId;
            CompetitionId = competitionId;
            Descending = descending;
            Page = page;
            PageSize = pageSize;
            var items = ItemsByPage.TryGetValue(page, out var pageItems) ? pageItems : Items;
            return Task.FromResult((items, TotalCount));
        }
    }

    private sealed class FakeTeamRepository(IReadOnlyList<TeamClean> teams) : ITeamRepository
    {
        public Task<TeamClean?> GetByIdAsync(long teamId, CancellationToken ct) =>
            Task.FromResult(teams.FirstOrDefault(team => team.TeamId == teamId));

        public Task<List<TeamClean>> GetByIdsAsync(List<long> teamIds, CancellationToken ct = default) =>
            Task.FromResult(teams.Where(team => teamIds.Contains(team.TeamId)).ToList());

        public Task<List<TeamClean>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(teams.ToList());
    }

    private sealed class StubMatchEventService : IMatchEventService
    {
        public Task<List<MatchEventResponse>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default) =>
            Task.FromResult(new List<MatchEventResponse>());
    }

    private sealed class StubLineupService : ILineupService
    {
        public Task<LineupResponse> GetLineupByMatchIdAsync(long matchId) =>
            Task.FromResult(new LineupResponse());

        public Task<LineupResponse?> GetLineupByMatchIdOrDefaultAsync(long matchId) =>
            Task.FromResult<LineupResponse?>(null);
    }

    private sealed class CapturingMatchService : IMatchService
    {
        public MatchSearchQuery? Query { get; private set; }

        public Task<MatchDetailResponse> GetMatchAsync(long matchId) =>
            throw new NotSupportedException();

        public Task<PagedResponse<MatchSummaryResponse>> SearchMatchesAsync(
            MatchSearchQuery query, CancellationToken ct = default)
        {
            Query = query;
            return Task.FromResult(new PagedResponse<MatchSummaryResponse>
            {
                Data = [],
                Pagination = new PaginationMetadata()
            });
        }
    }
}
