using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FotmobSync.Models.Clean
{
    /// <summary>
    /// Clean model cho trận đấu (đồng bộ từ FotMob fixture).
    /// </summary>
    [Table("matches")]
    public class MatchClean : BaseModel
    {
        [Column("match_id")]
        public long MatchId { get; set; }

        [Column("home_team_id")]
        public long HomeTeamId { get; set; }

        [Column("away_team_id")]
        public long AwayTeamId { get; set; }

        [Column("home_team_name")]
        public string? HomeTeamName { get; set; }

        [Column("away_team_name")]
        public string? AwayTeamName { get; set; }

        [Column("home_score")]
        public int? HomeScore { get; set; }

        [Column("away_score")]
        public int? AwayScore { get; set; }

        [Column("tournament_name")]
        public string? TournamentName { get; set; }

        [Column("league_id")]
        public long? LeagueId { get; set; }

        [Column("kickoff_utc")]
        public DateTime? KickoffUtc { get; set; }

        [Column("started")]
        public bool Started { get; set; }

        [Column("finished")]
        public bool Finished { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}
