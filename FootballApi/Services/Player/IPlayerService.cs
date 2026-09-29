namespace FootballApi.Services.Player;

using FootballApi.DTOs.Players;

public interface IPlayerService
{
    Task<PlayerProfileResponse> GetPlayerProfileAsync(int playerId, CancellationToken ct);
    Task<(IReadOnlyList<PlayerAppearanceResponse> Items, int TotalCount)> GetPlayerAppearancesAsync(
        long playerId, int page, int pageSize, CancellationToken ct);
    Task<(IReadOnlyList<PlayerSummaryResponse> Items, int TotalCount)> SearchPlayersAsync(
        PlayerSearchQuery query, CancellationToken ct);
}