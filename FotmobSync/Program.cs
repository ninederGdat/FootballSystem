using FotmobSync;
using FotmobSync.Clients;
using FotmobSync.Infrastructure;
using FotmobSync.Infrastructure.External;
using FotmobSync.Modules;
using FotmobSync.Jobs;
using FotmobSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;


var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();

//Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
    
    
//// Hosted services
//builder.Services.AddHostedService<Worker>();

// Register services
builder.Services.AddSingleton<SupabaseClientFactory>();
builder.Services.AddSingleton<FotmobTeamDataModule>();
builder.Services.AddScoped<IFotmobEtlService, FotmobEtlService>();
builder.Services.AddSingleton<PositionService>();
builder.Services.AddSingleton<MatchService>();
// Register FotmobBrowserClient
builder.Services.AddSingleton<FotmobBrowserClient>();
// Register FotmobClient
builder.Services.AddHttpClient<FotmobClient>();

// Register Job Quartz 
builder.Services.AddQuartz(q =>
{
   var jobKey = new JobKey("DailyFotmobSyncJob");
    q.AddJob<DailyFotmobSyncJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
     .ForJob(jobKey)
     .WithIdentity("DailyFotmobSyncJob-trigger")
     .StartNow()
     .WithCronSchedule("0 0/15 * * * ?")
     );                   
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();

// Set x-mas token for FotmobClient
var fotmobClient = host.Services.GetRequiredService<FotmobClient>();
fotmobClient.SetXMasToken("eyJib2R5Ijp7InVybCI6Ii9hcGkvZGF0YS9wbGF5ZXJEYXRhP2lkPTgwNzcyOSIsImNvZGUiOjE3NzgwMzgxMTUxOTUsImZvbyI6InByb2R1Y3Rpb246ZTRiODk0OTIxYzdlZmY4N2IyM2QxZTEyNzk1MjA2MzhjMmE1ZmVhMCJ9LCJzaWduYXR1cmUiOiIzQkU1RTcxNjI4NDBCMDhDQjNFNDkwMkQyMEZFNDkzRSJ9");
Console.WriteLine("🚀 FotmobSync Service is starting...");

host.Run();
