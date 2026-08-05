using FotmobSync.Infrastructure.External.Fotmob.Mapping;
using FotmobSync.Clients;
using FotmobSync.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Jobs;
using FotmobSync.Modules;
using FotmobSync.Options;
using FotmobSync.Services;
using FotmobSync.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using FotmobSync.Infrastructure.Resolvers.PositionRole;
using FotmobSync.Infrastructure.Resolvers.PlayingTime;
using FootballSystem.Shared.Infrastructure;
using FotmobSync.Mappers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<QuartzSyncOptions>(
    builder.Configuration.GetSection(QuartzSyncOptions.SectionName));

builder.Services.AddSingleton<SupabaseClientFactory>();
builder.Services.AddSingleton<FotmobTeamDataModule>();
builder.Services.AddSingleton<FotmobMatchDetailModule>();

// builder.Services.AddScoped<IFotmobEtlService, FotmobEtlService>();

// Sync Services
builder.Services.AddScoped<ITeamSyncService, TeamSyncService>();
builder.Services.AddScoped<IMatchSyncService, MatchService>();
builder.Services.AddScoped<ISquadSyncService, SquadSyncService>();
builder.Services.AddScoped<IPlayerSyncService, PlayerSyncService>();
builder.Services.AddScoped<ILineupSyncService, LineupSyncService>();
builder.Services.AddScoped<ILineupPlayerSyncService, LineupPlayerSyncService>();
builder.Services.AddScoped<ILineupBackfillService, LineupBackfillService>();
builder.Services.AddScoped<IMatchEventSyncService, MatchEventSyncService>();

// Lookup Services
builder.Services.AddSingleton<FormationService>();
builder.Services.AddSingleton<PositionService>();

builder.Services.AddSingleton<FotmobBrowserClient>();
builder.Services.AddHttpClient<FotmobClient>();

// Register workflow
builder.Services.AddScoped<ClubRefreshWorkflow>();
builder.Services.AddScoped<MatchLineupWorkflow>();

// Mapping Services
builder.Services.AddSingleton<IFotmobPositionMapper, FotmobPositionMapper>();
builder.Services.AddSingleton<IPositionRoleResolver,PositionRoleResolver>();
builder.Services.AddSingleton<IPlayingTimeResolver, PlayingTimeResolver>();


var quartzOptions = builder.Configuration
    .GetSection(QuartzSyncOptions.SectionName)
    .Get<QuartzSyncOptions>() ?? new QuartzSyncOptions();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DailyFotmobSyncJob");
    q.AddJob<DailyFotmobSyncJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(t => t
    .ForJob(jobKey)
    .WithIdentity("startup-trigger")
    .StartNow());

    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("cron-trigger")
        .WithCronSchedule(quartzOptions.CronSchedule));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = false;
    options.StartDelay = TimeSpan.FromSeconds(quartzOptions.SchedulerStartDelaySeconds);
});

var host = builder.Build();

var fotmobClient = host.Services.GetRequiredService<FotmobClient>();
fotmobClient.SetXMasToken("eyJib2R5Ijp7InVybCI6Ii9hcGkvZGF0YS9wbGF5ZXJEYXRhP2lkPTgwNzcyOSIsImNvZGUiOjE3NzgwMzgxMTUxOTUsImZvbyI6InByb2R1Y3Rpb246ZTRiODk0OTIxYzdlZmY4N2IyM2QxZTEyNzk1MjA2MzhjMmE1ZmVhMCJ9LCJzaWduYXR1cmUiOiIzQkU1RTcxNjI4NDBCMDhDQjNFNDkwMkQyMEZFNDkzRSJ9");

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("FotmobSync");
logger.LogInformation(
    "FotmobSync starting. Quartz: cron={Cron}, runOnStartup={RunOnStartup}, schedulerDelay={Delay}s",
    quartzOptions.CronSchedule,
    quartzOptions.RunOnStartup,
    quartzOptions.SchedulerStartDelaySeconds);


var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    logger.LogInformation("APPLICATION STARTED");
});

lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation("APPLICATION STOPPING");
});

var resolver = host.Services
    .GetRequiredService<IPositionRoleResolver>();

await resolver.InitializeAsync();

host.Run();
