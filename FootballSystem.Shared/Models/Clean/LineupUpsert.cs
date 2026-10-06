using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

[Table("lineups")]
public class LineupUpsert : BaseModel
{
    [Column("match_id")]
    public long MatchId { get; set; }
    [Column("type")]
    public string Type { get; set; } = default!;
    [Column("formation_id")]
    public long? FormationId { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("lineup_sync_attempts")]
public class LineupSyncAttemptClean : BaseModel
{
    [PrimaryKey("match_id", true)]
    public long MatchId { get; set; }
    [Column("attempts")]
    public int Attempts { get; set; }
    [Column("last_attempt_at")]
    public DateTime LastAttemptAt { get; set; }
}