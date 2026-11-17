using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

[Table("sync_runs")]
public class SyncRunClean : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("started_at")]
    public DateTime StartedAt { get; set; }
    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }
    [Column("status")]
    public string Status { get; set; } = "running";
    [Column("trigger")]
    public string? Trigger { get; set; }
    [Column("run_url")]
    public string? RunUrl { get; set; }
    [Column("pending_stubs")]
    public int? PendingStubs { get; set; }
}

[Table("sync_run_phases")]
public class SyncRunPhaseClean : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("run_id")]
    public long RunId { get; set; }
    [Column("phase_order")]
    public int PhaseOrder { get; set; }
    [Column("name")]
    public string Name { get; set; } = default!;
    [Column("status")]
    public string Status { get; set; } = default!;
    [Column("started_at")]
    public DateTime StartedAt { get; set; }
    [Column("finished_at")]
    public DateTime FinishedAt { get; set; }
    [Column("error_message")]
    public string? ErrorMessage { get; set; }
}