using FotmobSync.Services;
using FotmobSync.Workflows;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace FotmobSync.Jobs
{
    [DisallowConcurrentExecution]
    public class DailyFotmobSyncJob : IJob
    {
        private readonly ClubRefreshWorkflow _workflow;
        private readonly ILogger<DailyFotmobSyncJob> _logger;

        public DailyFotmobSyncJob(ClubRefreshWorkflow workflow,
                                ILogger<DailyFotmobSyncJob> logger)
        {
            _workflow = workflow;
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
                    await _workflow.ExecuteAsync(teamId);
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
