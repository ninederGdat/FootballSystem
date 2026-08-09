using FootballApi.DTOs.Responses;

namespace FootballApi.Services.MatchEvent;

public interface IMatchEventService
{
    Task<List<MatchEventItemResponse>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default);
}