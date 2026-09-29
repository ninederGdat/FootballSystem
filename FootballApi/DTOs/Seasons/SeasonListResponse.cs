namespace FootballApi.DTOs.Seasons;

public sealed class SeasonListResponse
{
    public IReadOnlyList<SeasonSummaryResponse> Data { get; set; } = [];
}