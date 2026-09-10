using FootballApi.Controllers;
using FootballApi.DTOs.Competitions;
using FootballApi.Repositories.Competition;
using FootballApi.Services.Competition;
using FootballSystem.Shared.Models.Clean;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FotmobSync.Tests.Competition;

public class CompetitionServiceTests
{
    [Fact]
    public async Task GetAllAsync_MapsCompetitionCleanToDto()
    {
        var repository = new FakeCompetitionRepository(
        [
            new CompetitionClean { CompetitionId = 42, Name = "Champions League" },
            new CompetitionClean { CompetitionId = 47, Name = "Premier League" }
        ]);

        var service = new CompetitionService(repository);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            item =>
            {
                Assert.Equal(42, item.Id);
                Assert.Equal("Champions League", item.Name);
            },
            item =>
            {
                Assert.Equal(47, item.Id);
                Assert.Equal("Premier League", item.Name);
            });
    }

    private sealed class FakeCompetitionRepository : ICompetitionRepository
    {
        private readonly IReadOnlyList<CompetitionClean> _items;

        public FakeCompetitionRepository(IReadOnlyList<CompetitionClean> items)
        {
            _items = items;
        }

        public Task<IReadOnlyList<CompetitionClean>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_items);
        }
    }
}

public class CompetitionsControllerTests
{
    [Fact]
    public async Task GetCompetitions_ReturnsOkWithDataWrapper()
    {
        var service = new FakeCompetitionService(
        [
            new CompetitionSummaryDto { Id = 42, Name = "Champions League" },
            new CompetitionSummaryDto { Id = 47, Name = "Premier League" }
        ]);

        var controller = new CompetitionsController(service);

        var result = await controller.GetCompetitions();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<CompetitionListResponse>(ok.Value);
        Assert.Equal(2, payload.Data.Count);
        Assert.Equal("Champions League", payload.Data[0].Name);
        Assert.Equal("Premier League", payload.Data[1].Name);
    }

    private sealed class FakeCompetitionService : ICompetitionService
    {
        private readonly IReadOnlyList<CompetitionSummaryDto> _items;

        public FakeCompetitionService(IReadOnlyList<CompetitionSummaryDto> items)
        {
            _items = items;
        }

        public Task<IReadOnlyList<CompetitionSummaryDto>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_items);
        }
    }
}
