using FootballApi.Common.Health;
using FootballApi.Repositories.Team;
using FootballSystem.Shared.Models.Clean;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FotmobSync.Tests.Health;

public class DatabaseHealthCheckTests
{
    private static DatabaseHealthCheck Create(ITeamRepository repo, TimeSpan? timeout = null)
        => new(repo, NullLogger<DatabaseHealthCheck>.Instance, timeout);

    [Fact]
    public async Task CheckHealthAsync_RepositoryResponds_ReturnsHealthy()
    {
        var check = Create(new FakeTeamRepository(() => Task.FromResult<TeamClean?>(null)));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_RepositoryThrows_ReturnsUnhealthyWithoutLeakingMessage()
    {
        var check = Create(new FakeTeamRepository(
            () => throw new InvalidOperationException("secret connection detail")));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.DoesNotContain("secret", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_RepositoryNeverReturns_ReturnsUnhealthyAfterTimeout()
    {
        var never = new TaskCompletionSource<TeamClean?>();
        var check = Create(new FakeTeamRepository(() => never.Task), TimeSpan.FromMilliseconds(50));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private sealed class FakeTeamRepository : ITeamRepository
    {
        private readonly Func<Task<TeamClean?>> _getById;

        public FakeTeamRepository(Func<Task<TeamClean?>> getById) => _getById = getById;

        public Task<TeamClean?> GetByIdAsync(long teamId, CancellationToken ct) => _getById();

        public Task<List<TeamClean>> GetByIdsAsync(List<long> teamIds, CancellationToken ct = default)
            => Task.FromResult(new List<TeamClean>());

        public Task<List<TeamClean>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(new List<TeamClean>());
    }
}