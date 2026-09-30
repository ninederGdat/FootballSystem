using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Common;
using FootballApi.DTOs.Players;
using FootballApi.DTOs.Transfers;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Team;
using FootballApi.Repositories.Transfer;
using FootballApi.Services.Season;

namespace FootballApi.Services.Transfer
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository _transferRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<TransferService> _logger;
        private readonly ISeasonService _seasonService;
        private readonly ITeamRepository _teamRepository;

        public TransferService(
               ITransferRepository transferRepository,
               IPlayerRepository playerRepository,
               ILogger<TransferService> logger,
               ISeasonService seasonService,
               ITeamRepository teamRepository
           )
        {
            _transferRepository = transferRepository;
            _playerRepository = playerRepository;
            _logger = logger;
            _seasonService = seasonService;
            _teamRepository = teamRepository;
        }

        public async Task<PlayerTransferStatusResponse> GetPlayerTransferStatusAsync(long playerId, long teamId, CancellationToken ct)
        {
            var transferHistory = await _transferRepository.GetTransfersByPlayerIdAsync(playerId, ct);

            var playerInfo = await _playerRepository.GetByIdAsync(playerId, ct);

            if (playerInfo is null)
            {
                throw new NotFoundException($"Player {playerId} not found.");
            }

            if (transferHistory is null || transferHistory.Count == 0)
            {
                _logger.LogInformation("Player {playerId} doesn't have any transfer in this season", playerId);

                return new PlayerTransferStatusResponse
                {
                    Status = playerInfo.TransferStatus,
                    CurrentTeam = "Chelsea",
                    PeriodEnd = playerInfo.ContractUntil ?? default,
                };
            }

            var latestTransfer = transferHistory.OrderByDescending(x => x.TransferDate).First();
            var status = new PlayerTransferStatusResponse
            {
                Status = MaptoStatus(latestTransfer.TransferType, teamId, latestTransfer.ToClubId),
                CurrentTeam = latestTransfer.ToClubName,
                PeriodEnd = latestTransfer.PeriodEnd.HasValue ? DateOnly.FromDateTime(latestTransfer.PeriodEnd.Value) : default
            };

            return status;
        }

        public async Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct)
        {
            // var transferHistory = _transferRepository.GetTransfersByPlayerIdAsync(playerId, ct);

            // if (transferHistory is null)
            // {
            //     throw new NotFoundException("Cannot find transfer history of Player {playerId} ");
            // }
            return [];

        }


        public async Task<(PagedResponse<TransferResponse> Items, int TotalCount)> SearchTransfersAsync(TransferQuery query, CancellationToken ct)
        {
            var fromDate = query.DateFrom;
            var toDate = query.DateTo;

            if (!string.IsNullOrWhiteSpace(query.Season))
            {
                var season = _seasonService.Resolve(query.Season);
                var range = SeasonDateRange.Constrain(season, fromDate, toDate);
                fromDate = range.FromDate;
                toDate = range.ToDateExclusive;
            }

            var (transfers, totalCount) = await _transferRepository.SearchAsync(
             query.PlayerName ?? string.Empty,
             query.PlayerId,
             query.FromClubId,
             query.ToClubId,
             query.OnLoan,
             query.ContractExtension,
             query.TransferType ?? string.Empty,
             fromDate,
             toDate,
             query.Page,
             query.PageSize,
             ct
            );

            if (transfers.Count == 0)
            {
                return (new PagedResponse<TransferResponse>
                {
                    Data = Array.Empty<TransferResponse>()
                }, totalCount);
            }

            var items = transfers.Select(transfer => new TransferResponse
            {
                PlayerId = checked((int)transfer.PlayerId),
                PlayerName = transfer.PlayerName,
                FromClubId = checked((int)transfer.FromClubId),
                FromClubName = transfer.FromClubName,
                ToClubId = checked((int)transfer.ToClubId),
                ToClubName = transfer.ToClubName,
                TransferDate = transfer.TransferDate,
                FromDate = transfer.PeriodStart,
                ToDate = transfer.PeriodEnd,
                TransferType = transfer.TransferType,
                OnLoan = transfer.OnLoan,
                ContractExtension = transfer.ContractExtension,
                Fee = transfer.FeeValue
            }).ToList();

            return (new PagedResponse<TransferResponse>
            {
                Data = items
            }, totalCount);
        }

        public async Task<TransferStatisticsResponse> GetTransferStatisticsAsync(
            TransferStatisticsQuery query,
            CancellationToken ct)
        {
            var season = string.IsNullOrWhiteSpace(query.Season)
                ? _seasonService.ResolveCurrent(DateTime.UtcNow)
                : _seasonService.Resolve(query.Season);
            var range = SeasonDateRange.Constrain(season);
            var teamId = query.TeamId ?? 8455;
            var team = await _teamRepository.GetByIdAsync(teamId, ct);

            if (team is null)
            {
                return TransferStatisticsMapper.Map(
                    teamId,
                    string.Empty,
                    season.Code,
                    0m,
                    0m,
                    0,
                    0,
                    0,
                    0);
            }

            var transfers = await _transferRepository.GetTransfersByDateRangeAsync(
                teamId,
                range.FromDate!.Value,
                range.ToDateExclusive!.Value,
                query.OnLoan,
                ct);
            var permanentBuys = transfers
                .Where(x => x.ToClubId == teamId && !x.OnLoan)
                .ToList();
            var permanentSales = transfers
                .Where(x => x.FromClubId == teamId && !x.OnLoan)
                .ToList();

            return TransferStatisticsMapper.Map(
                teamId,
                team.Name,
                season.Code,
                permanentBuys.Sum(x => x.FeeValue ?? 0m),
                permanentSales.Sum(x => x.FeeValue ?? 0m),
                permanentBuys.Count,
                permanentSales.Count,
                transfers.Count(x => x.ToClubId == teamId && x.OnLoan),
                transfers.Count(x => x.FromClubId == teamId && x.OnLoan));
        }



        public static string MaptoStatus(string transferType, long teamId, long toClubId)
        {

            return transferType switch
            {
                "on_loan" => "loaned",

                "contract" when toClubId == teamId
                => "current",
                "contract" => "transferred",

                _ => throw new InvalidOperationException(
            $"Unknown transfer type: {transferType}")
            };
        }


    }




}