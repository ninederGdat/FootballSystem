namespace FootballApi.Services.Player;

using FootballApi.DTOs.Common;
using FootballApi.DTOs.Players;

public interface IPlayerService
{
    Task<PlayerProfileResponse> GetPlayerProfileAsync(int playerId, CancellationToken ct);
    Task<PagedResponse<PlayerAppearanceResponse>> GetPlayerAppearancesAsync(
        long playerId, int page, int pageSize, CancellationToken ct);
    Task<PagedResponse<PlayerSummaryResponse>> SearchPlayersAsync(
        PlayerSearchQuery query, CancellationToken ct);
}