using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Match;

public interface IMatchRepository
{
    Task<MatchClean?> GetByMatchIdAsync(long matchId, CancellationToken ct = default);
    Task<List<MatchClean>> GetByMatchIdsAsync(IEnumerable<long> matchIds, CancellationToken ct = default);
}