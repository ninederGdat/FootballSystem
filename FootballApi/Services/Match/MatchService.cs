using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Responses;
using FootballApi.Repositories.Match;

namespace FootballApi.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _repository;

    public MatchService(IMatchRepository repository)
    {
        _repository = repository;
    }

    public async Task<MatchResponse> GetMatchAsync(long matchId)
    {
        var match = await _repository.GetByMatchIdAsync(matchId);

        if (match == null)
            throw new NotFoundException($"Không tìm thấy trận đấu với matchId {matchId}");

        return MatchResponse.FromClean(match);
    }
}