namespace FootballApi.DTOs.Sync;

public sealed class SyncRunResponse
{
    public long Id { get; set; }
    /// <summary>running | success | partial | failed | interrupted</summary>
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Trigger { get; set; }
    public string? RunUrl { get; set; }
    public int? PendingStubs { get; set; }
    public IReadOnlyList<string> FailedPhases { get; set; } = [];
}