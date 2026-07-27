namespace FootballApi.Repositories.Lineup;

public class PlayerAppearanceRecord
{
    public long MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public string? OpponentName { get; set; }
    public string? CompetitionName { get; set; }
    public bool IsStarter { get; set; }
    public string? PositionCode { get; set; }
    public int? RoleId { get; set; }
    public int? MinuteIn { get; set; }
    public int? MinuteOut { get; set; }
}