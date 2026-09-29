namespace FootballApi.DTOs.Players;

public class PlayerAppearanceResponse
{
    public long MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public string? OpponentName { get; set; }
    public string? CompetitionName { get; set; }
    public bool IsStarter { get; set; }
    public PlayerPositionResponse PositionPlayed { get; set; } = default!;
    public long? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? RoleShort { get; set; }
    public int? MinuteIn { get; set; }
    public int? MinuteOut { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
}