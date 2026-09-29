using FootballApi.DTOs.Matches;

namespace FootballApi.Services.MatchEvent;

public interface IMatchEventService
{
    Task<List<MatchEventResponse>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default);
}