using FootballApi.DTOs.Responses;

namespace FootballApi.Services;

public interface IMatchService
{
    Task<MatchResponse> GetMatchAsync(long matchId);
}