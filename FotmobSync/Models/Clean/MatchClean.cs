using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FotmobSync.Models.Clean;

/// <summary>
/// Clean model cho <c>public.matches</c> (một hàng theo góc nhìn đội <see cref="TeamId"/>).
/// </summary>
[Table("matches")]
public class MatchClean : BaseModel
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

    /// <summary><c>home</c> hoặc <c>away</c> (check constraint DB).</summary>
    [Column("home_or_away")]
    public string? HomeOrAway { get; set; }

    [Column("score_home")]
    public int? ScoreHome { get; set; }

    [Column("score_away")]
    public int? ScoreAway { get; set; }

    /// <summary><c>UPCOMING</c>, <c>ONGOING</c>, hoặc <c>FINISHED</c>.</summary>
    [Column("status")]
    public string Status { get; set; } = "UPCOMING";

    [Column("last_updated")]
    public DateTime? LastUpdated { get; set; }
}
