namespace FootballApi.DTOs.Responses;

public class LineupResponse
{
    public long LineupId { get; set; }
    public string Type { get; set; } = default!; // Predicted / Official
    public FormationDto? Formation { get; set; }
    public List<LineupPlayerResponse> Starters { get; set; } = new();
    public List<LineupPlayerResponse> Substitutes { get; set; } = new();
}

public class FormationDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}

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