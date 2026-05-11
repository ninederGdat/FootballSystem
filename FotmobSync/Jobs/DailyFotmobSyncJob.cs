using FotmobSync.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace FotmobSync.Jobs
{
    public class DailyFotmobSyncJob : IJob
    {
        private readonly IFotmobEtlService _fotmobEtlService;
        private readonly ILogger<DailyFotmobSyncJob> _logger;

        public DailyFotmobSyncJob(IFotmobEtlService fotmobEtlService, ILogger<DailyFotmobSyncJob> logger)
        {
            _fotmobEtlService = fotmobEtlService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting Daily Fotmob Sync Job at {Time}", DateTimeOffset.Now);
            try
            {
                // Example: Sync team with ID 8455, 8455 = Chelsea FC
                var teamsToSync = new List<int> { 8455 };

                foreach (var teamId in teamsToSync)
                {
                    _logger.LogInformation("Syncing team with ID {TeamId}", teamId);
                    await _fotmobEtlService.SyncTeamAndSquadAsync(teamId);
                }   
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Daily Fotmob Sync Job");
            }
            _logger.LogInformation("Completed Daily Fotmob Sync Job at {Time}", DateTimeOffset.Now);
        }
    }
}
