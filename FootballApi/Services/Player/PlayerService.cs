using FootballApi.DTOs.Players;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Position;
using FootballApi.Repositories.PositionRole;
using FootballApi.Repositories.Team;
using FootballApi.Repositories.Transfer;
using FootballApi.Services.Transfer;
using FootballSystem.Shared.Models.Clean;

namespace FootballApi.Services.Player
{
    /// <summary>
    /// Business exception used when a player does not exist.
    /// The controller translates this exception into HTTP 404.
    /// </summary>
    public class PlayerNotFoundException : Exception
    {
        public PlayerNotFoundException(long playerId) : base($"Player {playerId} not found.") { }
    }

    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IPositionRoleRepository _positionRoleRepository;
        private readonly ILineupRepository _lineupRepository;
        private readonly ITransferService _transferService;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(
            IPlayerRepository playerRepository,
            ITeamRepository teamRepository,
            IPositionRepository positionRepository,
            IPositionRoleRepository positionRoleRepository,
            ILineupRepository lineupRepository,
            ITransferService transferService,
            ILogger<PlayerService> logger)
        {
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
            _positionRepository = positionRepository;
            _positionRoleRepository = positionRoleRepository;
            _lineupRepository = lineupRepository;
            _transferService = transferService;
            _logger = logger;
        }

        // ---------------------------------------------------------------
        // GET /api/players/{playerId}
        // ---------------------------------------------------------------

        public async Task<PlayerProfileResponse> GetPlayerProfileAsync(int playerId, CancellationToken ct)
        {
            long id = playerId;
            var player = await _playerRepository.GetByIdAsync(id, ct);
            if (player is null)
            {
                _logger.LogWarning("Player {PlayerId} not found.", id);
                throw new PlayerNotFoundException(id);
            }

            var teamTask = _teamRepository.GetByIdAsync(player.TeamId, ct);

            // Skip database access if the player has no preferred position.
            var positionTask = player.PreferredPositionCode is not null
                ? _positionRepository.GetByCodesAsync(new List<string> { player.PreferredPositionCode }, ct)
                : Task.FromResult(new List<PositionClean>());

            var transferTask = _transferService.GetPlayerTransferStatusAsync(player.PlayerId, player.TeamId, ct);
            if (transferTask is null)
            {
                _logger.LogWarning("Player {PlayerId} not found.", id);
                transferTask = Task.FromResult(new PlayerTransferStatusResponse());
            }
            // Fire both independent database queries in parallel.
            // This reduces total latency compared to awaiting them sequentially.
            await Task.WhenAll(teamTask, positionTask, transferTask);

            var position = positionTask.Result.FirstOrDefault();
            var status = transferTask.Result;
            return MapToProfileDto(player, teamTask.Result, position, status);
        }

        /// <summary>
        /// Converts domain models into the API response DTO.
        /// No business logic should be added here.
        /// </summary>
        private static PlayerProfileResponse MapToProfileDto(PlayerClean player, TeamClean? team, PositionClean? position, PlayerTransferStatusResponse? status)
        {
            return new PlayerProfileResponse
            {
                PlayerId = player.PlayerId,
                Name = player.Name,
                ShirtNumber = player.ShirtNumber,
                DateOfBirth = player.DateOfBirth,
                Nationality = player.Nationality,
                Status = new PlayerStatusResponse
                {
                    Status = player.Status,
                    InjuryDescription = player.InjuryDescription
                },

                CurrentTeam = team is null ? null : new PlayerTeamResponse
                {
                    TeamId = (int)team.TeamId,
                    TeamName = team.Name
                },
                PreferredPosition = position is null ? null : new PlayerPositionResponse
                {
                    PositionCode = position.PositionCode,
                    PositionName = position.PositionName
                },
                Contract = new PlayerContractResponse
                {
                    ContractUntil = player.ContractUntil,
                    MarketValue = player.MarketValue
                },
                TransferStatus = status is null ? null : new PlayerTransferStatusResponse
                {
                    Status = status.Status,
                    CurrentTeam = status.CurrentTeam,
                    PeriodEnd = status.PeriodEnd
                }
            };
        }

        // ---------------------------------------------------------------
        // GET /api/players/{playerId}/appearances
        // ---------------------------------------------------------------
        // RoleId, MinuteIn, MinuteOut, plus a TotalCount for paging.
        public async Task<(IReadOnlyList<PlayerAppearanceResponse> Items, int TotalCount)> GetPlayerAppearancesAsync(
        long playerId, int page, int pageSize, CancellationToken ct)
        {
            var raw = await _lineupRepository.GetPlayerAppearancesAsync(playerId, page, pageSize, ct);

            if (raw.Items.Count == 0)
            {
                return (Array.Empty<PlayerAppearanceResponse>(), raw.TotalCount);
            }

            // Collect unique position codes so each position is loaded only once.
            var positionCodes = raw.Items.Select(i => i.PositionCode)
                                         .Where(c => c is not null).Select(c => c!).Distinct().ToList();

            var roleIds = raw.Items.Select(i => i.RoleId)
                                   .Where(r => r.HasValue).Select(r => r!.Value).Distinct().ToList();

            var positionsTask = positionCodes.Count > 0
                ? _positionRepository.GetByCodesAsync(positionCodes, ct)
                : Task.FromResult(new List<PositionClean>());

            var rolesTask = roleIds.Count > 0
                ? _positionRoleRepository.GetByIdsAsync(roleIds, ct)
                : Task.FromResult(new List<PositionRoleClean>());

            await Task.WhenAll(positionsTask, rolesTask);

            // Convert lookup data into dictionaries for O(1) access during mapping.
            var positionsByCode = positionsTask.Result.ToDictionary(p => p.PositionCode, p => p);
            var rolesById = rolesTask.Result.ToDictionary(r => r.Id, r => r);

            var appearances = raw.Items.Select(item =>
            {
                var position = item.PositionCode is not null
                    ? positionsByCode.GetValueOrDefault(item.PositionCode)
                    : null;

                return new PlayerAppearanceResponse
                {
                    MatchId = item.MatchId,
                    MatchDate = item.MatchDate,
                    OpponentName = item.OpponentName,
                    CompetitionName = item.CompetitionName,
                    IsStarter = item.IsStarter,
                    PositionPlayed = new PlayerPositionResponse
                    {
                        PositionCode = position?.PositionCode
                            ?? item.PositionCode
                            ?? string.Empty,
                        PositionName = position?.PositionName
                            ?? string.Empty
                    },

                    RoleId = item.RoleId,
                    RoleName = item.RoleId.HasValue ? rolesById.GetValueOrDefault(item.RoleId.Value)?.RoleName : null,
                    RoleShort = item.RoleId.HasValue ? rolesById.GetValueOrDefault(item.RoleId.Value)?.RoleShort : null,
                    MinuteIn = item.MinuteIn,
                    MinuteOut = item.MinuteOut,
                    Goals = item.Goals,
                    Assists = item.Assists
                };
            }).ToList();

            return (appearances, raw.TotalCount);
        }

        // ---------------------------------------------------------------
        // GET /api/players?search=&teamId=&positionCode=&nationality=&page=&pageSize=
        // ---------------------------------------------------------------
        // Search returns only player data.
        // Team names are resolved separately.
        public async Task<(IReadOnlyList<PlayerSummaryResponse> Items, int TotalCount)> SearchPlayersAsync(
           PlayerSearchQuery query, CancellationToken ct)

        {
            var (players, totalCount) = await _playerRepository.SearchAsync(
                query.Search,
                query.TeamId,
                query.PositionCode,
                query.Nationality,
                query.TransferStatus,
                query.Page,
                query.PageSize,
                ct);

            if (players.Count == 0)
            {
                return (Array.Empty<PlayerSummaryResponse>(), totalCount);
            }

            var teamIds = players.Select(p => p.TeamId).Distinct().ToList();
            var positionCodes = players
                .Select(p => p.PreferredPositionCode)
                .Where(c => c is not null)
                .Select(c => c!)
                .Distinct()
                .ToList();

            var teamsTask = _teamRepository.GetByIdsAsync(teamIds, ct);
            var positionsTask = positionCodes.Count > 0
                ? _positionRepository.GetByCodesAsync(positionCodes, ct)
                : Task.FromResult(new List<PositionClean>());

            await Task.WhenAll(teamsTask, positionsTask);

            var teamsById = teamsTask.Result.ToDictionary(t => t.TeamId, t => t);
            var positionsByCode = positionsTask.Result.ToDictionary(p => p.PositionCode, p => p);

            var items = players.Select(player =>
            {
                teamsById.TryGetValue(player.TeamId, out var team);
                PositionClean? position = player.PreferredPositionCode is not null
                    ? positionsByCode.GetValueOrDefault(player.PreferredPositionCode)
                    : null;

                return new PlayerSummaryResponse
                {
                    PlayerId = player.PlayerId,
                    Name = player.Name,
                    TeamName = team?.Name,
                    PositionCode = player.PreferredPositionCode,
                    PositionName = position?.PositionName,
                    Nationality = player.Nationality,
                    ShirtNumber = player.ShirtNumber,
                    Age = CalculateAge(player.DateOfBirth),
                    Status = player.Status,
                    //Format "90.00"
                    MarketValue = player.MarketValue is not null ? Math.Round(player.MarketValue.Value, 2) : null,
                    ContractUntil = player.ContractUntil,
                    TransferStatus = player.TransferStatus
                };
            }).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Computes age in whole years from a birth date, as of today (UTC).
        /// Returns null if the birth date is unknown.
        /// </summary>
        private static int? CalculateAge(DateOnly? dateOfBirth)
        {
            if (dateOfBirth is null) return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dateOfBirth.Value.Year;

            // Subtract one year if the birthday hasn't happened yet this year.
            if (dateOfBirth.Value > today.AddYears(-age))
                age--;

            return age;
        }
    }


}