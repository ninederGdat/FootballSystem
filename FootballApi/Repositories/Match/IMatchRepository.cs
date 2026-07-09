using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Repositories.Match;

public interface IMatchRepository
{
    Task<MatchClean?> GetByMatchIdAsync(long matchId);
}