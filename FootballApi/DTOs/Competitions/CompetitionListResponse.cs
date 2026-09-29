namespace FootballApi.DTOs.Competitions;

public sealed class CompetitionListResponse
{
    public IReadOnlyList<CompetitionSummaryResponse> Data { get; set; } = [];
}