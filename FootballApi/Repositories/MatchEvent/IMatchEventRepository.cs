using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.MatchEvent;

public interface IMatchEventRepository
{
    Task<List<MatchEventClean>> GetEventsByMatchIdAsync(long matchId, CancellationToken ct = default);

     /// <summary>Batch version cho PlayerAppearances (nhiều match cùng lúc) — tránh N+1.</summary>
    Task<List<MatchEventClean>> GetEventsByMatchIdsAsync(List<long> matchIds, CancellationToken ct = default);
}