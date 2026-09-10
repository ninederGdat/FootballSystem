namespace FootballApi.DTOs.Competitions;

public sealed class CompetitionSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CompetitionListResponse
{
    public IReadOnlyList<CompetitionSummaryDto> Data { get; set; } = [];
}
