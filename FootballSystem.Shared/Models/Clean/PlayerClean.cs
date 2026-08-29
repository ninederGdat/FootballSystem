using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballSystem.Shared.Models.Clean
{
    /// <summary>
    /// Clean model
    /// </summary>
    /// 
    [Table("players")]
    public class PlayerClean : BaseModel
    {
        [Column("player_id")]
        public long PlayerId { get; set; }                    // bigint PRIMARY KEY
        [Column("team_id")]
        public long TeamId { get; set; }                      // bigint, foreign key to teams
        [Column("name")]
        public string Name { get; set; } = string.Empty;      // text NOT NULL
        [Column("shirt_number")]
        public int? ShirtNumber { get; set; }                 // integer

        [Column("date_of_birth")]
        public DateOnly? DateOfBirth { get; set; }            // date
        [Column("nationality")]
        public string? Nationality { get; set; }              // text (country code hoặc tên quốc gia)
        [Column("contract_until")]
        public DateOnly? ContractUntil { get; set; }          // date
        [Column("market_value")]
        public decimal? MarketValue { get; set; }             // numeric(12,2)

        [Column("status")]
        public string Status { get; set; } = "Healthy";       // text DEFAULT 'Healthy'
        [Column("injury_description")]
        public string? InjuryDescription { get; set; }       // text, injury line when status is Injured
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }               // timestamptz

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }             // timestamptz
        [Column("preferred_position_code")]
        public string? PreferredPositionCode { get; set; }    // text, foreign key to positions
        [Column("transfer_status")]
        public string TransferStatus { get; set; } = "current"; // "current" | "loaned" | "transferred"
    }
}
