using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

[Table("lineup_players")]
public class LineupPlayerUpsert : BaseModel
{
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