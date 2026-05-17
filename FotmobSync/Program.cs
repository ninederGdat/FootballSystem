using FotmobSync.Clients;
using FotmobSync.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Jobs;
using FotmobSync.Modules;
using FotmobSync.Options;
using FotmobSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

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
builder.Services.AddScoped<IFotmobEtlService, FotmobEtlService>();
builder.Services.AddSingleton<PositionService>();
builder.Services.AddSingleton<MatchService>();
builder.Services.AddSingleton<FotmobBrowserClient>();
builder.Services.AddHttpClient<FotmobClient>();

var quartzOptions = builder.Configuration
    .GetSection(QuartzSyncOptions.SectionName)
    .Get<QuartzSyncOptions>() ?? new QuartzSyncOptions();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DailyFotmobSyncJob");
    q.AddJob<DailyFotmobSyncJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts =>
    {
        var trigger = opts
            .WithIdentity("DailyFotmobSyncJob-trigger")
            .ForJob(jobKey)
            .WithCronSchedule(quartzOptions.CronSchedule);

        // StartNow() runs the full ETL (Playwright + per-player delays) before the app feels "ready".
        if (quartzOptions.RunOnStartup)
            trigger.StartNow();
    });
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

host.Run();
