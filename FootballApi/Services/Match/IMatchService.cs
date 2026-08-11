using FootballApi.DTOs.Matches;
using FootballApi.DTOs.Responses;

namespace FootballApi.Services;

public interface IMatchService
{
    Task<MatchResponse> GetMatchAsync(long matchId);

     Task<(IReadOnlyList<MatchSummaryDTO> Items, int TotalCount)> SearchMatchesAsync(
        MatchSearchQuery query, CancellationToken ct = default);
}