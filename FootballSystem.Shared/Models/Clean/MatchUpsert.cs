using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

/// <summary>
/// Model ghi cho <c>public.matches</c> từ fixtures sync.
/// Cố ý KHÔNG có penalty_home / penalty_away: hai cột đó do match-detail sync ghi,
/// nếu nằm trong model upsert này thì giá trị null sẽ ghi đè mỗi chu kỳ.
/// </summary>
[Table("matches")]
public class MatchUpsert : BaseModel
{
    [Column("match_id")]
    public long MatchId { get; set; }

    [Column("team_id")]
    public long? TeamId { get; set; }

    [Column("opponent_team_id")]
    public long? OpponentTeamId { get; set; }

    [Column("opponent_name")]
    public string OpponentName { get; set; } = string.Empty;

    [Column("competition_id")]
    public long? CompetitionId { get; set; }

    [Column("competition_name")]
    public string? CompetitionName { get; set; }

    [Column("match_date")]
    public DateTime MatchDate { get; set; }

    [Column("home_or_away")]
    public string? HomeOrAway { get; set; }

    [Column("score_home")]
    public int? ScoreHome { get; set; }

    [Column("score_away")]
    public int? ScoreAway { get; set; }

    [Column("status")]
    public string Status { get; set; } = "UPCOMING";

    [Column("last_updated")]
    public DateTime? LastUpdated { get; set; }
}