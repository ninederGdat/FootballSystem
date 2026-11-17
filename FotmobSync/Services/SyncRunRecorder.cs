using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using Supabase.Postgrest;

namespace FotmobSync.Services;

public interface ISyncRunRecorder
{
    Task StartAsync();
    Task RecordPhaseAsync(string name, bool succeeded, DateTime startedAt, string? error);
    Task RecordPendingStubsAsync();
    void MarkAborted();
    Task FinishAsync(bool completed);
}

/// <summary>
/// Writes sync_runs / sync_run_phases. Bookkeeping must never break the sync itself,
/// so every database call is wrapped and only logged on failure. Writes take no
/// CancellationToken on purpose: a cancelled run still has to record its final status.
/// </summary>
public class SyncRunRecorder : ISyncRunRecorder
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<SyncRunRecorder> _logger;

    private long? _runId;
    private int _phaseOrder;
    private bool _anyPhaseFailed;
    private bool _aborted;

    public SyncRunRecorder(SupabaseClientFactory factory, ILogger<SyncRunRecorder> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task StartAsync()
    {
        try
        {
            var run = new SyncRunClean
            {
                StartedAt = DateTime.UtcNow,
                Status = SyncRunStatus.Running,
                Trigger = Env("GITHUB_EVENT_NAME"),
                RunUrl = BuildRunUrl()
            };
            var response = await _supabase.From<SyncRunClean>().Insert(run);
            _runId = response.Models.FirstOrDefault()?.Id;
            _logger.LogInformation("Sync run {RunId} started", _runId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create sync run record; continuing without it");
        }
    }

    public async Task RecordPhaseAsync(string name, bool succeeded, DateTime startedAt, string? error)
    {
        // State used for the final status is updated even if the DB write below fails.
        _phaseOrder++;
        if (!succeeded) _anyPhaseFailed = true;
        if (_runId is null) return;

        try
        {
            await _supabase.From<SyncRunPhaseClean>().Insert(new SyncRunPhaseClean
            {
                RunId = _runId.Value,
                PhaseOrder = _phaseOrder,
                Name = name,
                Status = succeeded ? "success" : "failed",
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow,
                ErrorMessage = SyncRunStatus.Truncate(error)
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not record phase {Phase}", name);
        }
    }

    public async Task RecordPendingStubsAsync()
    {
        if (_runId is null) return;
        var id = _runId.Value;
        try
        {
            var pending = await _supabase.From<PlayerClean>()
                .Filter("is_stub", Constants.Operator.Equals, "true")
                .Count(Constants.CountType.Exact);

            await _supabase.From<SyncRunClean>()
                .Where(x => x.Id == id)
                .Set(x => x.PendingStubs, pending)
                .Update();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not record pending stub count");
        }
    }

    public void MarkAborted() => _aborted = true;

    public async Task FinishAsync(bool completed)
    {
        if (_runId is null) return;
        var id = _runId.Value;
        var status = SyncRunStatus.Resolve(completed && !_aborted, _anyPhaseFailed);
        try
        {
            await _supabase.From<SyncRunClean>()
                .Where(x => x.Id == id)
                .Set(x => x.Status, status)
                .Set(x => x.FinishedAt, DateTime.UtcNow)
                .Update();
            _logger.LogInformation("Sync run {RunId} finished: {Status}", id, status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not finish sync run {RunId}", id);
        }
    }

    private static string? Env(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? BuildRunUrl()
    {
        var server = Env("GITHUB_SERVER_URL");
        var repo = Env("GITHUB_REPOSITORY");
        var runId = Env("GITHUB_RUN_ID");
        return server is null || repo is null || runId is null
            ? null
            : $"{server}/{repo}/actions/runs/{runId}";
    }
}