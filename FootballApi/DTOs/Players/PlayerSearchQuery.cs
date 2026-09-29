namespace FootballApi.DTOs.Players;

public record PlayerSearchQuery
{
    public string? Search { get; init; }
    public int? TeamId { get; init; }
    public string? PositionCode { get; init; }
    public string? Nationality { get; init; }
    public string? TransferStatus { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}