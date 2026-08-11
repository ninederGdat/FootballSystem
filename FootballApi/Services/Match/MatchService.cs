using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Matches;
using FootballApi.DTOs.Responses;
using FootballApi.Repositories.Match;
using FootballApi.Services.MatchEvent;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _repository;
    private readonly IMatchEventService _eventService;
    private readonly ILineupService _lineupService;

    public MatchService(IMatchRepository repository, IMatchEventService eventService,
                        ILineupService lineupService)
    {
        _repository = repository;
        _eventService = eventService;
        _lineupService = lineupService;
    }

    public async Task<MatchResponse> GetMatchAsync(long matchId)
    {
        var match = await _repository.GetByMatchIdAsync(matchId);

        if (match == null)
            throw new NotFoundException($"Không tìm thấy trận đấu với matchId {matchId}");

        var eventsTask = _eventService.GetEventsByMatchIdAsync(matchId);
        var lineupTask = _lineupService.GetLineupByMatchIdOrDefaultAsync(matchId);

        await Task.WhenAll(eventsTask, lineupTask);

        return MatchResponse.FromClean(match, eventsTask.Result, lineupTask.Result);
    }

    public async Task<(IReadOnlyList<MatchSummaryDTO> Items, int TotalCount)> SearchMatchesAsync(
         MatchSearchQuery query, CancellationToken ct = default)
    {
        var (matches, totalCount) = await _repository.SearchAsync(
            query.FromDate,
            query.ToDate,
            query.Opponent,
            query.Status,
            query.Page,
            query.PageSize,
            ct);

        var items = matches.Select(MapToSummaryDto).ToList();

        return (items, totalCount);
    }

    private static MatchSummaryDTO MapToSummaryDto(MatchClean match)
    {
        return new MatchSummaryDTO
        {
            MatchId = match.MatchId,
            MatchDate = match.MatchDate,
            OpponentName = match.OpponentName,
            HomeOrAway = match.HomeOrAway,
            CompetitionName = match.CompetitionName,
            ScoreHome = match.ScoreHome,
            ScoreAway = match.ScoreAway,
            Status = match.Status
        };
    }
}