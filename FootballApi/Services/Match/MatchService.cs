using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Matches;
using FootballApi.DTOs.Responses;
using FootballApi.Repositories.Match;
using FootballApi.Services.MatchEvent;
using FootballApi.Services.Season;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _repository;
    private readonly IMatchEventService _eventService;
    private readonly ILineupService _lineupService;
    private readonly ISeasonService _seasonService;

    public MatchService(IMatchRepository repository, IMatchEventService eventService,
                        ILineupService lineupService, ISeasonService seasonService)
    {
        _repository = repository;
        _eventService = eventService;
        _lineupService = lineupService;
        _seasonService = seasonService;
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
        var fromDate = query.FromDate;
        var toDate = query.ToDate;

        if (!string.IsNullOrWhiteSpace(query.Season))
        {
            var season = _seasonService.Resolve(query.Season);
            fromDate = fromDate is null ? season.StartDate : fromDate.Value > season.StartDate ? fromDate.Value : season.StartDate;
            toDate = toDate is null ? season.EndDate : toDate.Value < season.EndDate ? toDate.Value : season.EndDate;
        }

        var (matches, totalCount) = await _repository.SearchAsync(
            fromDate,
            toDate,
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