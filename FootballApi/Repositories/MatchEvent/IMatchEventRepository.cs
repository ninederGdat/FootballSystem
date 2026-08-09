using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.MatchEvent;

public interface IMatchEventRepository
{
    Task<List<MatchEventClean>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default);
}