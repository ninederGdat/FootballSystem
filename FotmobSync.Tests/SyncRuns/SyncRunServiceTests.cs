using FootballApi.Common.Exceptions;
using FootballApi.Controllers;
using FootballApi.DTOs.Sync;
using FootballApi.Repositories.SyncRun;
using FootballApi.Services.SyncRun;
using FootballSystem.Shared.Models.Clean;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FotmobSync.Tests.SyncRuns;

public class SyncRunServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    private static SyncRunService Create(FakeSyncRunRepository repo) =>
        new(repo, new FixedTimeProvider(Now));

    private static SyncRunClean Run(long id, string status, TimeSpan age, TimeSpan? duration = null) => new()
    {
        Id = id,
        Status = status,
        StartedAt = DateTime.SpecifyKind(Now - age, DateTimeKind.Utc),
        FinishedAt = duration is null ? null : DateTime.SpecifyKind(Now - age + duration.Value, DateTimeKind.Utc),
        Trigger = "schedule"
    };

    [Fact]
    public async Task GetRecentAsync_RunningWithinThreshold_StaysRunning()
    {
        var repo = new FakeSyncRunRepository([Run(1, "running", TimeSpan.FromMinutes(35))]);

        var result = await Create(repo).GetRecentAsync(null);

        Assert.Equal("running", result[0].Status);
    }

    [Fact]
    public async Task GetRecentAsync_RunningPastThreshold_IsInterrupted()
    {
        var repo = new FakeSyncRunRepository([Run(1, "running", TimeSpan.FromMinutes(36))]);

        var result = await Create(repo).GetRecentAsync(null);

        Assert.Equal("interrupted", result[0].Status);
        Assert.Null(result[0].DurationSeconds);
    }

    [Fact]
    public async Task GetRecentAsync_OldFinishedRun_KeepsStoredStatusAndDuration()
    {
        var repo = new FakeSyncRunRepository(
            [Run(1, "success", TimeSpan.FromDays(3), TimeSpan.FromSeconds(756))]);

        var result = await Create(repo).GetRecentAsync(null);

        Assert.Equal("success", result[0].Status);
        Assert.Equal(756, result[0].DurationSeconds);
    }

    [Fact]
    public async Task GetRecentAsync_MapsFailedPhasesToTheirRunInPhaseOrder()
    {
        var repo = new FakeSyncRunRepository(
            [Run(2, "partial", TimeSpan.FromHours(1), TimeSpan.FromMinutes(13)),
             Run(1, "success", TimeSpan.FromDays(7), TimeSpan.FromMinutes(12))])
        {
            FailedPhases =
            [
                new SyncRunPhaseClean { RunId = 2, PhaseOrder = 6, Name = "PHASE 3" },
                new SyncRunPhaseClean { RunId = 2, PhaseOrder = 5, Name = "PHASE 2" }
            ]
        };

        var result = await Create(repo).GetRecentAsync(null);

        Assert.Equal(["PHASE 2", "PHASE 3"], result[0].FailedPhases);
        Assert.Empty(result[1].FailedPhases);
    }

    [Fact]
    public async Task GetRecentAsync_NoLimit_UsesDefault()
    {
        var repo = new FakeSyncRunRepository([]);

        await Create(repo).GetRecentAsync(null);

        Assert.Equal(SyncRunService.DefaultLimit, repo.LastLimit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public async Task GetRecentAsync_LimitOutOfRange_ThrowsBadRequest(int limit)
    {
        var service = Create(new FakeSyncRunRepository([]));

        await Assert.ThrowsAsync<BadRequestException>(() => service.GetRecentAsync(limit));
    }

    [Fact]
    public async Task GetRecentAsync_UnspecifiedKind_IsTreatedAsProcessLocalTime()
    {
        // Supabase.Postgrest 4.1.0 returns timestamptz as Unspecified local wall-clock.
        var instant = Now - TimeSpan.FromHours(2);
        var run = Run(1, "success", TimeSpan.FromHours(2), TimeSpan.FromMinutes(10));
        run.StartedAt = DateTime.SpecifyKind(
            TimeZoneInfo.ConvertTimeFromUtc(instant, TimeZoneInfo.Local),
            DateTimeKind.Unspecified);
        var repo = new FakeSyncRunRepository([run]);

        var result = await Create(repo).GetRecentAsync(null);

        Assert.Equal(DateTimeKind.Utc, result[0].StartedAt.Kind);
        Assert.Equal(instant, result[0].StartedAt);
    }

    [Fact]
    public void ToUtc_Unspecified_UsesTheGivenZone()
    {
        var utcPlus7 = TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
        var wall = new DateTime(2026, 11, 17, 17, 14, 36, DateTimeKind.Unspecified);

        var result = SyncRunService.ToUtc(wall, utcPlus7);

        Assert.Equal(new DateTime(2026, 11, 17, 10, 14, 36), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void ToUtc_Utc_IsUnchanged()
    {
        var utc = new DateTime(2026, 11, 17, 10, 14, 36, DateTimeKind.Utc);

        var result = SyncRunService.ToUtc(utc, TimeZoneInfo.Utc);

        Assert.Equal(utc, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public async Task Controller_WrapsRunsInDataEnvelope()
    {
        var repo = new FakeSyncRunRepository([Run(1, "success", TimeSpan.FromHours(1), TimeSpan.FromMinutes(5))]);
        var controller = new SyncController(Create(repo));

        var result = await controller.GetRuns(null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SyncRunListResponse>(ok.Value);
        Assert.Single(payload.Data);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FixedTimeProvider(DateTime utcNow) => _now = new DateTimeOffset(utcNow, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
    }

    private sealed class FakeSyncRunRepository : ISyncRunRepository
    {
        private readonly IReadOnlyList<SyncRunClean> _runs;
        public IReadOnlyList<SyncRunPhaseClean> FailedPhases { get; init; } = [];
        public int? LastLimit { get; private set; }

        public FakeSyncRunRepository(IReadOnlyList<SyncRunClean> runs) => _runs = runs;

        public Task<IReadOnlyList<SyncRunClean>> GetRecentRunsAsync(int limit, CancellationToken ct = default)
        {
            LastLimit = limit;
            return Task.FromResult(_runs);
        }

        public Task<IReadOnlyList<SyncRunPhaseClean>> GetFailedPhasesAsync(
            IReadOnlyCollection<long> runIds, CancellationToken ct = default)
            => Task.FromResult(FailedPhases);
    }
}