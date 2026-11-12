using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballApi.Repositories.Lineup;

/// <summary>Read model cho view <c>public.player_appearances_v</c> (lineup_players ⨝ lineups ⨝ matches).</summary>
[Table("player_appearances_v")]
public class PlayerAppearanceViewClean : BaseModel
{
    [Column("lineup_player_id")] public long LineupPlayerId { get; set; }
    [Column("player_id")] public long PlayerId { get; set; }
    [Column("lineup_id")] public long LineupId { get; set; }
    [Column("match_id")] public long MatchId { get; set; }
    [Column("match_date")] public DateTime MatchDate { get; set; }
    [Column("opponent_name")] public string? OpponentName { get; set; }
    [Column("competition_name")] public string? CompetitionName { get; set; }
    [Column("played_for_team_id")] public long? PlayedForTeamId { get; set; }
    [Column("is_starter")] public bool? IsStarter { get; set; }
    [Column("position_code")] public string? PositionCode { get; set; }
    [Column("role_id")] public long? RoleId { get; set; }
    [Column("minute_in")] public int? MinuteIn { get; set; }
    [Column("minute_out")] public int? MinuteOut { get; set; }
}