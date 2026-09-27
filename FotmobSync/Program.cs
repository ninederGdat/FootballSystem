using FotmobSync.Infrastructure.External.Fotmob.Mapping;
using FotmobSync.Clients;
using FotmobSync.Infrastructure.External;
using FotmobSync.Jobs;
using FotmobSync.Modules;
using FotmobSync.Options;
using FotmobSync.Services;
using FotmobSync.Workflows;
using Quartz;
using FotmobSync.Infrastructure.Resolvers.PositionRole;
using FotmobSync.Infrastructure.Resolvers.PlayingTime;
using FootballSystem.Shared.Infrastructure;
using FotmobSync.Infrastructure.Resolvers.TransferStatus;

var builder = Host.CreateApplicationBuilder(args);

var runOnce = args.Contains(
    "--run-once",
    StringComparer.OrdinalIgnoreCase);


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<QuartzSyncOptions>(
    builder.Configuration.GetSection(QuartzSyncOptions.SectionName));

builder.Services.Configure<FotmobOptions>(
    builder.Configuration.GetSection(FotmobOptions.SectionName));

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
builder.Services.AddScoped<ITransferSyncService, TransferSyncService>();
builder.Services.AddScoped<ITransferStatusSyncService, TransferStatusSyncService>();
// Lookup Services
builder.Services.AddSingleton<FormationService>();
builder.Services.AddSingleton<PositionService>();

builder.Services.AddSingleton<FotmobBrowserClient>();
builder.Services.AddHttpClient<FotmobClient>();

// Register workflow
builder.Services.AddScoped<ClubRefreshWorkflow>();
builder.Services.AddScoped<MatchLineupWorkflow>();
builder.Services.AddScoped<IFotmobSyncRunner, FotmobSyncRunner>();
// Mapping Services
builder.Services.AddSingleton<IFotmobPositionMapper, FotmobPositionMapper>();
builder.Services.AddSingleton<IPositionRoleResolver, PositionRoleResolver>();
builder.Services.AddSingleton<IPlayingTimeResolver, PlayingTimeResolver>();
builder.Services.AddSingleton<ITransferStatusResolver, TransferStatusResolver>();

var quartzOptions = builder.Configuration
    .GetSection(QuartzSyncOptions.SectionName)
    .Get<QuartzSyncOptions>() ?? new QuartzSyncOptions();

var fotmobOptions = builder.Configuration
    .GetSection(FotmobOptions.SectionName)
    .Get<FotmobOptions>() ?? new FotmobOptions();

if (!runOnce)
{
    builder.Services.AddQuartz(q =>
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

    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = false;
        options.StartDelay =
            TimeSpan.FromSeconds(
                quartzOptions.SchedulerStartDelaySeconds);
    });
}

var host = builder.Build();

var fotmobClient = host.Services.GetRequiredService<FotmobClient>();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("FotmobSync");
logger.LogInformation(
    "FotmobSync starting. Mode={Mode}, Quartz: cron={Cron}, runOnStartup={RunOnStartup}, schedulerDelay={Delay}s",
    runOnce ? "OneShot" : "Scheduled",
    quartzOptions.CronSchedule,
    quartzOptions.RunOnStartup,
    quartzOptions.SchedulerStartDelaySeconds);

if (string.IsNullOrWhiteSpace(fotmobOptions.XMasToken))
{
    logger.LogCritical(
        "Fotmob:XMasToken is not configured. Set it via appsettings.Development.json (local) " +
        "or the FOTMOB__XMAS_TOKEN environment variable / GitHub Secret (production).");

    Environment.ExitCode = 1;
    return;
}

fotmobClient.SetXMasToken(fotmobOptions.XMasToken);

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

if (runOnce)
{
    try
    {
        await resolver.InitializeAsync();

        using var scope = host.Services.CreateScope();

        var syncRunner = scope.ServiceProvider
            .GetRequiredService<IFotmobSyncRunner>();

        logger.LogInformation(
            "Starting one-shot Fotmob synchronization.");

        await syncRunner.RunAsync();

        logger.LogInformation(
            "One-shot Fotmob synchronization completed successfully.");

        return;
    }
    catch (Exception ex)
    {
        logger.LogCritical(
            ex,
            "One-shot Fotmob synchronization failed.");

        Environment.ExitCode = 1;
        return;
    }
}

await resolver.InitializeAsync();
await host.RunAsync();
