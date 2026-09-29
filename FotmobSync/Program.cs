using FotmobSync.Extensions;
using FotmobSync.Infrastructure.Resolvers.PositionRole;
using FotmobSync.Options;
using FotmobSync.Runners;

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

builder.Services.AddFotmobSyncServices();

var quartzOptions = builder.Configuration
    .GetSection(QuartzSyncOptions.SectionName)
    .Get<QuartzSyncOptions>() ?? new QuartzSyncOptions();

if (!runOnce)
    builder.Services.AddFotmobQuartzScheduling(quartzOptions);

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("FotmobSync");
logger.LogInformation(
    "FotmobSync starting. Mode={Mode}, Quartz: cron={Cron}, runOnStartup={RunOnStartup}, schedulerDelay={Delay}s",
    runOnce ? "OneShot" : "Scheduled",
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

if (runOnce)
{
    Environment.ExitCode = await OneShotRunner.RunOnceAsync(host.Services, logger);
    return;
}

if (!OneShotRunner.InitializeFotmobClient(host.Services, logger))
{
    Environment.ExitCode = 1;
    return;
}

var resolver = host.Services
    .GetRequiredService<IPositionRoleResolver>();

await resolver.InitializeAsync();
await host.RunAsync();
