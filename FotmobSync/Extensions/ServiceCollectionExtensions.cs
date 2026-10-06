using FotmobSync.Clients;
using FotmobSync.Infrastructure.External;
using FotmobSync.Infrastructure.External.Fotmob.Mapping;
using FotmobSync.Infrastructure.Resolvers.PlayingTime;
using FotmobSync.Infrastructure.Resolvers.PositionRole;
using FotmobSync.Infrastructure.Resolvers.TransferStatus;
using FotmobSync.Jobs;
using FotmobSync.Modules;
using FotmobSync.Options;
using FotmobSync.Services;
using FotmobSync.Workflows;
using FootballSystem.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FotmobSync.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFotmobSyncServices(this IServiceCollection services)
    {
        services.AddSingleton<SupabaseClientFactory>();
        services.AddSingleton<FotmobTeamDataModule>();
        services.AddSingleton<FotmobMatchDetailModule>();

        // builder.Services.AddScoped<IFotmobEtlService, FotmobEtlService>();

        // Sync Services
        services.AddScoped<ITeamSyncService, TeamSyncService>();
        services.AddScoped<IMatchSyncService, MatchService>();
        services.AddScoped<ISquadSyncService, SquadSyncService>();
        services.AddScoped<IPlayerSyncService, PlayerSyncService>();
        services.AddScoped<ILineupSyncService, LineupSyncService>();
        services.AddScoped<ILineupPlayerSyncService, LineupPlayerSyncService>();
        services.AddScoped<ILineupBackfillService, LineupBackfillService>();
        services.AddScoped<IMatchEventSyncService, MatchEventSyncService>();
        services.AddScoped<ITransferSyncService, TransferSyncService>();
        services.AddScoped<ITransferStatusSyncService, TransferStatusSyncService>();
        services.AddScoped<IPlayerStubService, PlayerStubService>();
        // Lookup Services
        services.AddSingleton<FormationService>();
        services.AddSingleton<PositionService>();

        services.AddSingleton<FotmobBrowserClient>();
        services.AddHttpClient<FotmobClient>();

        // Register workflow
        services.AddScoped<ClubRefreshWorkflow>();
        services.AddScoped<MatchLineupWorkflow>();
        services.AddScoped<IFotmobSyncRunner, FotmobSyncRunner>();
        // Mapping Services
        services.AddSingleton<IFotmobPositionMapper, FotmobPositionMapper>();
        services.AddSingleton<IPositionRoleResolver, PositionRoleResolver>();
        services.AddSingleton<IPlayingTimeResolver, PlayingTimeResolver>();
        services.AddSingleton<ITransferStatusResolver, TransferStatusResolver>();

        return services;
    }

    public static IServiceCollection AddFotmobQuartzScheduling(
        this IServiceCollection services,
        QuartzSyncOptions quartzOptions)
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("DailyFotmobSyncJob");

            q.AddJob<DailyFotmobSyncJob>(
                opts => opts.WithIdentity(jobKey));

            if (quartzOptions.RunOnStartup)
            {
                q.AddTrigger(t => t
                    .ForJob(jobKey)
                    .WithIdentity("startup-trigger")
                    .StartNow());
            }

            q.AddTrigger(t => t
                .ForJob(jobKey)
                .WithIdentity("cron-trigger")
                .WithCronSchedule(quartzOptions.CronSchedule));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = false;
            options.StartDelay =
                TimeSpan.FromSeconds(
                    quartzOptions.SchedulerStartDelaySeconds);
        });

        return services;
    }
}