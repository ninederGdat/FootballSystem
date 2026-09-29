using FootballApi.DTOs.Common;
using FootballApi.DTOs.Matches;

namespace FootballApi.Services;

public interface IMatchService
{
    Task<MatchDetailResponse> GetMatchAsync(long matchId);

    Task<PagedResponse<MatchSummaryResponse>> SearchMatchesAsync(
       MatchSearchQuery query, CancellationToken ct = default);
}