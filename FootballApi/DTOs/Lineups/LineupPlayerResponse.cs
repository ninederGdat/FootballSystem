namespace FootballApi.DTOs.Lineups;

public class LineupPlayerResponse
{
    public long PlayerId { get; set; }
    public string PlayerName { get; set; } = default!;
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public long? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? RoleShort { get; set; }
    public int? ShirtNumber { get; set; }
    public int? MinuteIn { get; set; }
    public int? MinuteOut { get; set; }
    public double? CustomX { get; set; }
    public double? CustomY { get; set; }
}