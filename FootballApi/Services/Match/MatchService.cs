using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Responses;
using FootballApi.Repositories.Match;
using FootballApi.Services.MatchEvent;

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
}