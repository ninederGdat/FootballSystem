namespace FootballApi.Services.Player;

using FootballApi.DTOs.Players;

public interface IPlayerService
{
    Task<PlayerProfileDTO> GetPlayerProfileAsync(int playerId, CancellationToken ct);
    Task<(IReadOnlyList<PlayerAppearanceDTO> Items, int TotalCount)> GetPlayerAppearancesAsync(
        long playerId, int page, int pageSize, CancellationToken ct);
    Task<(IReadOnlyList<PlayerSummaryDTO> Items, int TotalCount)> SearchPlayersAsync(
        PlayerSearchQuery query, CancellationToken ct);
}

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