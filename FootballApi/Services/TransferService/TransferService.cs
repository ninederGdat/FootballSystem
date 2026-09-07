using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Players;
using FootballApi.Repositories.Player;
using FootballApi.Repositories.Transfer;

namespace FootballApi.Services.Transfer
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository _transferRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<TransferService> _logger;

        public TransferService(
               ITransferRepository transferRepository,
               IPlayerRepository playerRepository,
               ILogger<TransferService> logger
           )
        {
            _transferRepository = transferRepository;
            _playerRepository = playerRepository;
            _logger = logger;
        }

        public async Task<PlayerTransferStatusDTO> GetPlayerTransferStatusAsync(long playerId, long teamId, CancellationToken ct)
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

                return new PlayerTransferStatusDTO
                {
                    Status = playerInfo.TransferStatus,
                    CurrentTeam = "Chelsea",
                    PeriodEnd = playerInfo.ContractUntil ?? default,
                };
            }

            var latestTransfer = transferHistory.OrderByDescending(x => x.TransferDate).First();
            var status = new PlayerTransferStatusDTO
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