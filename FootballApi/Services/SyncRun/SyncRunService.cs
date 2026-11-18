using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Sync;
using FootballApi.Repositories.SyncRun;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Services.SyncRun;

public class SyncRunService : ISyncRunService
{
    public const int DefaultLimit = 5;
    public const int MaxLimit = 20;

    /// <summary>
    /// A 'running' run older than this is reported as 'interrupted'. The sync workflow
    /// times out at 30 minutes (fotmob-sync.yml); a normal run takes about 13 minutes.
    /// </summary>
    public static readonly TimeSpan InterruptedAfter = TimeSpan.FromMinutes(35);

    private const string Running = "running";
    private const string Interrupted = "interrupted";

    private readonly ISyncRunRepository _repository;
    private readonly TimeProvider _timeProvider;

    public SyncRunService(ISyncRunRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<SyncRunResponse>> GetRecentAsync(int? limit, CancellationToken ct = default)
    {
        var take = limit ?? DefaultLimit;
        if (take < 1 || take > MaxLimit)
            throw new BadRequestException($"limit must be between 1 and {MaxLimit}.");

        var runs = await _repository.GetRecentRunsAsync(take, ct);
        if (runs.Count == 0) return [];

        var failedPhases = await _repository.GetFailedPhasesAsync(runs.Select(r => r.Id).ToList(), ct);
        var failedByRun = failedPhases
            .GroupBy(p => p.RunId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.OrderBy(p => p.PhaseOrder).Select(p => p.Name).ToList());

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        return runs.Select(run => Map(run, failedByRun, now)).ToList();
    }

    private static SyncRunResponse Map(
        SyncRunClean run,
        IReadOnlyDictionary<long, IReadOnlyList<string>> failedByRun,
        DateTime nowUtc)
    {
        var started = AsUtc(run.StartedAt);
        DateTime? finished = run.FinishedAt is { } f ? AsUtc(f) : null;

        var status = run.Status == Running && nowUtc - started > InterruptedAfter
            ? Interrupted
            : run.Status;

        return new SyncRunResponse
        {
            Id = run.Id,
            Status = status,
            StartedAt = started,
            FinishedAt = finished,
            DurationSeconds = finished is { } end ? (int)Math.Max(0, (end - started).TotalSeconds) : null,
            Trigger = run.Trigger,
            RunUrl = run.RunUrl,
            PendingStubs = run.PendingStubs,
            FailedPhases = failedByRun.TryGetValue(run.Id, out var names) ? names : []
        };
    }

    // The client may hand back Local or Unspecified kinds; always expose UTC.
    private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
        : value.ToUniversalTime();
}