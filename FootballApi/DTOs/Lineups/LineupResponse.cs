namespace FootballApi.DTOs.Lineups;

public class LineupResponse
{
    public long LineupId { get; set; }
    public string Type { get; set; } = default!; // Predicted / Official
    public FormationResponse? Formation { get; set; }
    public List<LineupPlayerResponse> Starters { get; set; } = new();
    public List<LineupPlayerResponse> Substitutes { get; set; } = new();
}