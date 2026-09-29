namespace FootballApi.DTOs.Lineups;

public class FormationResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}