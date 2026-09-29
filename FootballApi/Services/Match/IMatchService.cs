using FootballApi.DTOs.Matches;

namespace FootballApi.Services;

public interface IMatchService
{
    Task<MatchDetailResponse> GetMatchAsync(long matchId);

    Task<(IReadOnlyList<MatchSummaryResponse> Items, int TotalCount)> SearchMatchesAsync(
       MatchSearchQuery query, CancellationToken ct = default);
}