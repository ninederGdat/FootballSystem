using FotmobSync.Clients;
using FotmobSync.Infrastructure.Resolvers.PositionRole;
using FotmobSync.Options;
using FotmobSync.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FotmobSync.Runners;

public static class OneShotRunner
{
    public static async Task<int> RunOnceAsync(IServiceProvider services, ILogger logger)
    {
        if (!InitializeFotmobClient(services, logger))
            return 1;

        var resolver = services
            .GetRequiredService<IPositionRoleResolver>();

        try
        {
            await resolver.InitializeAsync();

            using var scope = services.CreateScope();

            var syncRunner = scope.ServiceProvider
                .GetRequiredService<IFotmobSyncRunner>();

            logger.LogInformation(
                "Starting one-shot Fotmob synchronization.");

            await syncRunner.RunAsync();

            logger.LogInformation(
                "One-shot Fotmob synchronization completed successfully.");

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "One-shot Fotmob synchronization failed.");

            return 1;
        }
    }

    public static bool InitializeFotmobClient(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var fotmobOptions = configuration
            .GetSection(FotmobOptions.SectionName)
            .Get<FotmobOptions>() ?? new FotmobOptions();

        var fotmobClient = services.GetRequiredService<FotmobClient>();

        if (string.IsNullOrWhiteSpace(fotmobOptions.XMasToken))
        {
            logger.LogCritical(
                "Fotmob:XMasToken is not configured. Set it via appsettings.Development.json (local) " +
                "or the FOTMOB__XMASTOKEN environment variable / GitHub Secret (production).");

            return false;
        }

        fotmobClient.SetXMasToken(fotmobOptions.XMasToken);
        return true;
    }
}