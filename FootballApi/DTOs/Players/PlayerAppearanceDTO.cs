namespace FootballApi.DTOs.Players;

public class PlayerAppearanceDTO
{
    public long MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public string? OpponentName { get; set; }
    public string? CompetitionName { get; set; }
    public bool IsStarter { get; set; }
    public PlayerPositionDTO PositionPlayed { get; set; } = default!;
    public long? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? RoleShort { get; set; }
    public int? MinuteIn { get; set; }
    public int? MinuteOut { get; set; }
}

public class PlayerAppearancesResult
{
    public long PlayerId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<PlayerAppearanceDTO> Appearances { get; set; } = [];
}