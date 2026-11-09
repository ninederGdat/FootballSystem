using FootballApi.Controllers;
using FootballApi.DTOs.Common;
using FootballApi.DTOs.Players;
using FootballApi.DTOs.Transfers;
using FootballApi.Repositories.Lineup;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Position;
using FootballApi.Repositories.PositionRole;
using FootballApi.Repositories.Team;
using FootballApi.Repositories.Transfer;
using FootballApi.Services.Player;
using FootballApi.Services.Transfer;
using FootballSystem.Shared.Models.Clean;
using FootballSystem.Shared.Players;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FotmobSync.Tests.Players;

public class PlayerSearchApiTests
{
    private const long TeamId = 8455;

    public static TheoryData<string> PlayerFilters => new()
    {
        "search", "positionCode", "nationality", "status", "transferStatus"
    };

    [Theory]
    [MemberData(nameof(PlayerFilters))]
    public async Task SearchPlayersAsync_FilterIsForwarded(string filter)
    {
        var repository = new RecordingPlayerRepository();
        var service = CreateService(repository);
        var query = new PlayerSearchQuery { TeamId = TeamId };

        query = filter switch
        {
            "search" => query with { Search = "Alex" },
            "positionCode" => query with { PositionCode = "MID" },
            "nationality" => query with { Nationality = "England" },
            "status" => query with { Status = "Healthy" },
            "transferStatus" => query with { TransferStatus = PlayerRelation.LoanedIn },
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };

        await service.SearchPlayersAsync(query, default);

        Assert.Equal(query.Search, repository.Search);
        Assert.Equal(query.TeamId, repository.TeamId);
        Assert.Equal(query.PositionCode, repository.PositionCode);
        Assert.Equal(query.Nationality, repository.Nationality);
        Assert.Equal(query.Status, repository.Status);
        Assert.Equal(query.TransferStatus, repository.Relation);
    }

    [Theory]
    [MemberData(nameof(PlayerFilters))]
    public async Task SearchPlayersAsync_FilterWithoutValue_PassesNullOrCurrentRelation(string filter)
    {
        var repository = new RecordingPlayerRepository();
        await CreateService(repository).SearchPlayersAsync(
            new PlayerSearchQuery { TeamId = TeamId }, default);

        switch (filter)
        {
            case "search":
                Assert.Null(repository.Search);
                break;
            case "positionCode":
                Assert.Null(repository.PositionCode);
                break;
            case "nationality":
                Assert.Null(repository.Nationality);
                break;
            case "status":
                Assert.Null(repository.Status);
                break;
            case "transferStatus":
                Assert.Equal(PlayerRelation.Current, repository.Relation);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
        }
    }

    [Fact]
    public async Task SearchPlayersAsync_NoOptionalFilters_PassesNullValuesAndCurrentRelation()
    {
        var repository = new RecordingPlayerRepository();
        var service = CreateService(repository);

        await service.SearchPlayersAsync(new PlayerSearchQuery { TeamId = TeamId }, default);

        Assert.Null(repository.Search);
        Assert.Null(repository.PositionCode);
        Assert.Null(repository.Nationality);
        Assert.Null(repository.Status);
        Assert.Equal(TeamId, repository.TeamId);
        Assert.Equal(PlayerRelation.Current, repository.Relation);
    }

    [Fact]
    public async Task SearchPlayersAsync_CombinedFiltersWithNoMatches_ReturnsEmptyPage()
    {
        var repository = new RecordingPlayerRepository { TotalCount = 0 };
        var service = CreateService(repository);
        var query = new PlayerSearchQuery
        {
            Search = "Nobody",
            TeamId = TeamId,
            PositionCode = "DEF",
            Nationality = "Unknown",
            Status = "Injured",
            TransferStatus = PlayerRelation.Transferred,
            Page = 2,
            PageSize = 3
        };

        var result = await service.SearchPlayersAsync(query, default);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
        Assert.Equal(query.Search, repository.Search);
        Assert.Equal(query.TeamId, repository.TeamId);
        Assert.Equal(query.PositionCode, repository.PositionCode);
        Assert.Equal(query.Nationality, repository.Nationality);
        Assert.Equal(query.Status, repository.Status);
        Assert.Equal(query.TransferStatus, repository.Relation);
        Assert.Equal(query.Page, repository.Page);
        Assert.Equal(query.PageSize, repository.PageSize);
    }

    [Fact]
    public async Task SearchPlayers_ControllerWithoutTeamId_ReturnsBadRequest()
    {
        // REVIEW: Missing teamId currently returns 400; defaulting to a configured team is another possible contract.
        var service = new CapturingPlayerService();
        var controller = new PlayersController(service);

        var result = await controller.SearchPlayers(
            search: null,
            teamId: null,
            positionCode: null,
            nationality: null,
            status: null,
            transferStatus: null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Null(service.Query);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("Loaned")]
    public async Task SearchPlayers_ControllerWithUnknownTransferStatus_ReturnsBadRequest(string status)
    {
        var service = new CapturingPlayerService();
        var controller = new PlayersController(service);

        var result = await controller.SearchPlayers(
            search: null,
            teamId: TeamId,
            positionCode: null,
            nationality: null,
            status: null,
            transferStatus: status);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Null(service.Query);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchPlayers_ControllerWithNonPositiveTeamId_PassesValueToService(long teamId)
    {
        // REVIEW: The API accepts non-positive team IDs; validating them or using a default team could be preferable.
        var service = new CapturingPlayerService();
        var controller = new PlayersController(service);

        var result = await controller.SearchPlayers(
            search: null,
            teamId: teamId,
            positionCode: null,
            nationality: null,
            status: null,
            transferStatus: null);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(teamId, service.Query!.TeamId);
    }

    [Theory]
    [InlineData(0, 0, 1, 8)]
    [InlineData(-2, 101, 1, 8)]
    [InlineData(3, 101, 3, 8)]
    [InlineData(3, 20, 3, 20)]
    public async Task SearchPlayers_ControllerNormalizesPageParameters(
        int requestedPage, int requestedPageSize, int expectedPage, int expectedPageSize)
    {
        var service = new CapturingPlayerService();
        var controller = new PlayersController(service);

        await controller.SearchPlayers(
            search: null,
            teamId: TeamId,
            positionCode: null,
            nationality: null,
            status: null,
            transferStatus: null,
            page: requestedPage,
            pageSize: requestedPageSize);

        Assert.Equal(expectedPage, service.Query!.Page);
        Assert.Equal(expectedPageSize, service.Query.PageSize);
    }

    [Theory]
    [InlineData(1, 8, 17, 3)]
    [InlineData(3, 8, 17, 3)]
    [InlineData(4, 8, 17, 3)]
    [InlineData(1, 20, 17, 1)]
    [InlineData(1, 8, 0, 0)]
    public async Task SearchPlayersAsync_Page_ReturnsCorrectPaginationMetadata(
        int page, int pageSize, int totalItems, int totalPages)
    {
        var repository = new RecordingPlayerRepository
        {
            Items = [Player(1), Player(2)],
            TotalCount = totalItems
        };

        var result = await CreateService(repository).SearchPlayersAsync(new PlayerSearchQuery
        {
            TeamId = TeamId,
            Page = page,
            PageSize = pageSize
        }, default);

        Assert.Equal(page, result.Pagination.Page);
        Assert.Equal(pageSize, result.Pagination.PageSize);
        Assert.Equal(totalItems, result.Pagination.TotalItems);
        Assert.Equal(totalPages, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task SearchPlayersAsync_RepositoryOrderAcrossPages_HasNoDuplicates()
    {
        var firstPage = new RecordingPlayerRepository
        {
            ItemsByPage = new Dictionary<int, List<PlayerClean>>
            {
                [1] = [Player(1), Player(2)]
            },
            TotalCount = 4
        };
        var secondPage = new RecordingPlayerRepository
        {
            ItemsByPage = new Dictionary<int, List<PlayerClean>>
            {
                [2] = [Player(3), Player(4)]
            },
            TotalCount = 4
        };
        var query = new PlayerSearchQuery { TeamId = TeamId, PageSize = 2 };

        var pageOne = await CreateService(firstPage).SearchPlayersAsync(query, default);
        var pageTwo = await CreateService(secondPage).SearchPlayersAsync(query with { Page = 2 }, default);
        var ids = pageOne.Data.Concat(pageTwo.Data).Select(player => player.PlayerId).ToList();

        Assert.Equal(new long[] { 1, 2, 3, 4 }, ids);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public async Task SearchPlayersAsync_NoPosition_MapsNullableFields()
    {
        var repository = new RecordingPlayerRepository
        {
            Items = [Player(1, preferredPositionCode: null)],
            TotalCount = 1
        };

        var result = await CreateService(repository).SearchPlayersAsync(
            new PlayerSearchQuery { TeamId = TeamId }, default);
        var item = Assert.Single(result.Data);

        Assert.Null(item.PositionCode);
        Assert.Null(item.PositionName);
        Assert.Null(item.Nationality);
        Assert.Null(item.ShirtNumber);
        Assert.Null(item.Age);
        Assert.Null(item.MarketValue);
    }

    [Fact]
    public async Task SearchPlayersAsync_PositionNotFound_MapsPositionNameAsNull()
    {
        var repository = new RecordingPlayerRepository
        {
            Items = [Player(1, preferredPositionCode: "UNKNOWN")],
            TotalCount = 1
        };

        var result = await CreateService(repository).SearchPlayersAsync(
            new PlayerSearchQuery { TeamId = TeamId }, default);
        var item = Assert.Single(result.Data);

        Assert.Equal("UNKNOWN", item.PositionCode);
        Assert.Null(item.PositionName);
    }

    [Fact]
    public async Task SearchPlayersAsync_TeamAndRelationScope_ArePassedToRepository()
    {
        var repository = new RecordingPlayerRepository();

        await CreateService(repository).SearchPlayersAsync(new PlayerSearchQuery
        {
            TeamId = TeamId,
            TransferStatus = PlayerRelation.LoanedOut
        }, default);

        Assert.Equal(TeamId, repository.TeamId);
        Assert.Equal(PlayerRelation.LoanedOut, repository.Relation);
    }

    private static PlayerService CreateService(RecordingPlayerRepository playerRepository) =>
        new(
            playerRepository,
            new FakeTeamRepository(),
            new FakePositionRepository(),
            new StubPositionRoleRepository(),
            new StubLineupRepository(),
            new StubTransferService(),
            new FakeTransferRepository(),
            NullLogger<PlayerService>.Instance);

    private static PlayerClean Player(long id, string? preferredPositionCode = null) => new()
    {
        PlayerId = id,
        TeamId = TeamId,
        CurrentTeamId = TeamId,
        Name = $"Player {id}",
        PreferredPositionCode = preferredPositionCode,
        TransferStatus = "current",
        IsStub = false
    };

    private sealed class RecordingPlayerRepository : IPlayerRepository
    {
        public string? Search { get; private set; }
        public long TeamId { get; private set; }
        public string? PositionCode { get; private set; }
        public string? Nationality { get; private set; }
        public string? Status { get; private set; }
        public PlayerRelation? Relation { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; init; }
        public List<PlayerClean> Items { get; init; } = [];
        public Dictionary<int, List<PlayerClean>> ItemsByPage { get; init; } = [];

        public Task<PlayerClean?> GetByIdAsync(long playerId, CancellationToken ct) =>
            Task.FromResult<PlayerClean?>(null);

        public Task<List<PlayerClean>> GetByIdsAsync(IEnumerable<long> playerIds, CancellationToken ct) =>
            Task.FromResult(new List<PlayerClean>());

        public Task<(List<PlayerClean> Items, int TotalCount)> SearchAsync(
            string? search,
            long teamId,
            string? positionCode,
            string? nationality,
            string? status,
            PlayerRelation? relation,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            Search = search;
            TeamId = teamId;
            PositionCode = positionCode;
            Nationality = nationality;
            Status = status;
            Relation = relation;
            Page = page;
            PageSize = pageSize;
            var items = ItemsByPage.TryGetValue(page, out var pageItems) ? pageItems : Items;
            return Task.FromResult((items, TotalCount));
        }
    }

    private sealed class FakeTeamRepository : ITeamRepository
    {
        private readonly List<TeamClean> _teams =
        [
            new TeamClean { TeamId = TeamId, Name = "Tracked FC" }
        ];

        public Task<TeamClean?> GetByIdAsync(long teamId, CancellationToken ct) =>
            Task.FromResult(_teams.FirstOrDefault(team => team.TeamId == teamId));

        public Task<List<TeamClean>> GetByIdsAsync(List<long> teamIds, CancellationToken ct = default) =>
            Task.FromResult(_teams.Where(team => teamIds.Contains(team.TeamId)).ToList());

        public Task<List<TeamClean>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(_teams.ToList());
    }

    private sealed class FakePositionRepository : IPositionRepository
    {
        public Task<List<PositionClean>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(new List<PositionClean>());

        public Task<List<PositionClean>> GetByCodesAsync(List<string> positionCodes, CancellationToken ct = default) =>
            Task.FromResult(new List<PositionClean>());
    }

    private sealed class StubPositionRoleRepository : IPositionRoleRepository
    {
        public Task<List<PositionRoleClean>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(new List<PositionRoleClean>());

        public Task<List<PositionRoleClean>> GetByIdsAsync(List<int> roleIds, CancellationToken ct = default) =>
            Task.FromResult(new List<PositionRoleClean>());
    }

    private sealed class StubLineupRepository : ILineupRepository
    {
        public Task<LineupClean?> GetLineupByMatchIdAsync(long matchId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<FormationClean?> GetFormationByIdAsync(long formationId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<LineupPlayerClean>> GetLineupPlayersAsync(long lineupId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<PlayerClean>> GetPlayersByIdsAsync(List<long> playerIds, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<(List<LineupPlayerClean> Items, int TotalCount)> GetLineupPlayersByPlayerIdAsync(
            long playerId, int page, int pageSize, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<(List<PlayerAppearanceRecord> Items, int TotalCount)> GetPlayerAppearancesAsync(
            long playerId, int page, int pageSize, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<LineupClean>> GetByIdsAsync(List<long> lineupIds, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class StubTransferService : ITransferService
    {
        public Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<PlayerTransferStatusResponse> GetPlayerTransferStatusAsync(
            long playerId, long teamId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<(PagedResponse<TransferResponse> Items, int TotalCount)> SearchTransfersAsync(
            TransferQuery query, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<TransferStatisticsResponse> GetTransferStatisticsAsync(
            TransferStatisticsQuery query, CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private sealed class FakeTransferRepository : ITransferRepository
    {
        public Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct = default) =>
            Task.FromResult(new List<TransferClean>());

        public Task<IReadOnlyList<TransferClean>> GetTransfersByDateRangeAsync(
            long teamId, DateTime from, DateTime toExclusive, bool? onLoan, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<TransferClean>>([]);

        public Task<(IReadOnlyList<TransferClean> Items, int TotalCount)> SearchAsync(
            string playerName,
            long? playerId,
            long? fromClubId,
            long? toClubId,
            long? teamId,
            string? direction,
            IReadOnlyCollection<long>? internalClubIds,
            bool? onLoan,
            bool? contractExtension,
            string transferType,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize,
            CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Dictionary<long, TransferClean>> GetLatestByPlayerIdsAsync(
            IEnumerable<long> playerIds, CancellationToken ct = default) =>
            Task.FromResult(new Dictionary<long, TransferClean>());
    }

    private sealed class CapturingPlayerService : IPlayerService
    {
        public PlayerSearchQuery? Query { get; private set; }

        public Task<PlayerProfileResponse> GetPlayerProfileAsync(int playerId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<PagedResponse<PlayerAppearanceResponse>> GetPlayerAppearancesAsync(
            long playerId, int page, int pageSize, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<PagedResponse<PlayerSummaryResponse>> SearchPlayersAsync(
            PlayerSearchQuery query, CancellationToken ct)
        {
            Query = query;
            return Task.FromResult(new PagedResponse<PlayerSummaryResponse>
            {
                Data = [],
                Pagination = new PaginationMetadata()
            });
        }
    }

}
