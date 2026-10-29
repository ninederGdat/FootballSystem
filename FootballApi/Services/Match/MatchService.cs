using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Common;
using FootballApi.DTOs.Matches;
using FootballApi.DTOs.Lineups;
using FootballApi.Repositories.Match;
using FootballApi.Services.MatchEvent;
using FootballApi.Services.Season;
using FootballSystem.Shared.Models.Clean;
using FootballApi.Repositories.Team;

namespace FootballApi.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _repository;
    private readonly IMatchEventService _eventService;
    private readonly ILineupService _lineupService;
    private readonly ISeasonService _seasonService;
    private readonly ITeamRepository _teamRepository;

    public MatchService(IMatchRepository repository, IMatchEventService eventService,
                        ILineupService lineupService, ISeasonService seasonService,
                  ITeamRepository teamRepository)
    {
        _repository = repository;
        _eventService = eventService;
        _lineupService = lineupService;
        _seasonService = seasonService;
        _teamRepository = teamRepository;
    }

    public async Task<MatchDetailResponse> GetMatchAsync(long matchId)
    {
        var match = await _repository.GetByMatchIdAsync(matchId);

        if (match == null)
            throw new NotFoundException($"Không tìm thấy trận đấu với matchId {matchId}");

        var eventsTask = _eventService.GetEventsByMatchIdAsync(matchId);
        var lineupTask = _lineupService.GetLineupByMatchIdOrDefaultAsync(matchId);

        await Task.WhenAll(eventsTask, lineupTask);

        return MatchDetailResponse.FromClean(match, eventsTask.Result, lineupTask.Result);
    }

    public async Task<PagedResponse<MatchSummaryResponse>> SearchMatchesAsync(
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

        // teams chỉ chứa các CLB được theo dõi 
        if (query.TeamId is { } contextTeamId &&
            await _teamRepository.GetByIdAsync(contextTeamId, ct) is null)
            throw new BadRequestException($"teamId {contextTeamId} is not a tracked team.");

        var (matches, totalCount) = await _repository.SearchAsync(
            fromDate,
            toDate,
            query.Opponent,
            query.Status,
            query.TeamId,
            query.CompetitionId,
            query.Page,
            query.PageSize,
            ct);

        // Trận giữa hai CLB theo dõi chỉ có 1 hàng, team_id có thể là CLB còn lại.
        // Khi xem theo một CLB, đảo hàng đó về đúng góc nhìn của CLB đang xem.
        var flippedTeamIds = query.TeamId is { } viewTeamId
                    ? matches.Where(m => NeedsFlip(m, viewTeamId)).Select(m => m.TeamId!.Value).Distinct().ToList()
                    : new List<long>();

        var teamsById = flippedTeamIds.Count > 0
                    ? (await _teamRepository.GetByIdsAsync(flippedTeamIds, ct)).ToDictionary(t => t.TeamId)
                    : new Dictionary<long, TeamClean>();

        var items = matches.Select(m => MapToSummaryDto(m, query.TeamId, teamsById)).ToList();

        return new PagedResponse<MatchSummaryResponse>
        {
            Data = items,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            }
        };
    }

    private static bool NeedsFlip(MatchClean match, long viewTeamId) => match.TeamId is not null && match.TeamId != viewTeamId && match.OpponentTeamId == viewTeamId;

    private static MatchSummaryResponse MapToSummaryDto(
        MatchClean match, long? viewTeamId, IReadOnlyDictionary<long, TeamClean> teamsById)
    {
        var flip = viewTeamId is { } v && NeedsFlip(match, v);

        return new MatchSummaryResponse
        {
            MatchId = match.MatchId,
            TeamId = flip ? viewTeamId : match.TeamId,
            OpponentTeamId = flip ? match.TeamId : match.OpponentTeamId,
            MatchDate = match.MatchDate,

            OpponentName = flip
               ? teamsById.GetValueOrDefault(match.TeamId!.Value)?.Name ?? "Unknown"
               : match.OpponentName,
            HomeOrAway = flip ? FlipHomeOrAway(match.HomeOrAway) : match.HomeOrAway,
            CompetitionName = match.CompetitionName,
            ScoreHome = match.ScoreHome,
            ScoreAway = match.ScoreAway,
            Status = match.Status
        };
    }

    private static string? FlipHomeOrAway(string? value) => value switch
    {
        "home" => "away",
        "away" => "home",
        _ => value
    };
}