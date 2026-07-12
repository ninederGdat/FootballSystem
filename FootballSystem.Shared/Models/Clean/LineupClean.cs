using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

/// <summary>
/// Clean model for lineup
/// </summary>
[Table("lineups")]
public class LineupClean : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")]
    public long Id { get; set; }
    [Column("match_id")]
    public long MatchId { get; set; }
    [Column("type")]

    /// <summary>
    /// Predicted / Official
    /// </summary>
    public string Type { get; set; } = default!;
    [Column("formation_id")]
    public long? FormationId { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}



/// <summary>
/// Clean model for lineup player
/// </summary>
[Table("lineup_players")]
public class LineupPlayerClean : BaseModel
{
    [PrimaryKey("id")]

    [Column("id")]
    public long Id { get; set; }

    [Column("lineup_id")]
    public long? LineupId { get; set; }

    [Column("player_id")]
    public long PlayerId { get; set; }

    [Column("position_code")]
    public string? PositionCode { get; set; }

    [Column("role_id")]
    public long? RoleId { get; set; }

    [Column("shirt_number")]
    public int? ShirtNumber { get; set; }

    [Column("is_starter")]
    public bool IsStarter { get; set; }

    [Column("minute_in")]
    public int? MinuteIn { get; set; }

    [Column("minute_out")]
    public int? MinuteOut { get; set; }

    [Column("custom_x")]
    public double? CustomX { get; set; }

    [Column("custom_y")]
    public double? CustomY { get; set; }

}