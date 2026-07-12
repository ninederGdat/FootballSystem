using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean
{
    /// <summary>
    /// Clean model 
    /// </summary>
    /// 
    [Table("teams")]
    public class TeamClean : BaseModel
    {
        [Column("team_id")]
        public long TeamId { get; set; }                    // bigint PRIMARY KEY
        [Column("name")]
        public string Name { get; set; } = string.Empty;    // text NOT NULL

        [Column("logo_url")]
        public string? LogoUrl { get; set; }                // text
        [Column("coach_name")]
        public string? CoachName { get; set; }              // text
        [Column("coach_nationality")]
        public string? CoachNationality { get; set; }       // text

        //public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
