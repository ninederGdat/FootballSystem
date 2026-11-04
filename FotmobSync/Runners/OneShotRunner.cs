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
            if (Environment.GetEnvironmentVariable("PG_DEBUG") == "1")
                await PgProbeAsync(services.GetRequiredService<IConfiguration>());
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


    // TODO(debug): tạm thời, xoá sau khi tìm ra nguyên nhân PGRST102 trên GitHub Actions.
    // Gọi thẳng PostgREST bằng HttpClient (bỏ qua postgrest-csharp) để biết lỗi nằm ở thư viện hay ở đường truyền.
    private static async Task PgProbeAsync(IConfiguration configuration)
    {
        Console.WriteLine(
            $"[PG-PROBE] runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription} " +
            $"os={System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        foreach (var t in new[] { typeof(Supabase.Client), typeof(Supabase.Postgrest.Client)
})
        {
            var a = t.Assembly;
            var info = a.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;
            Console.WriteLine($"[PG-PROBE] assembly={a.GetName().Name} version={a.GetName().Version} info={info}");
        }

        var url = configuration["Supabase:Url"]!.TrimEnd('/');
        var key = configuration["Supabase:ServiceRoleKey"]!;
        var row = "{\"position_code\":\"GK\",\"position_name\":\"Keeper\",\"x_coord\":0.5,\"y_coord\":0.5,\"is_goalkeeper\":true}";

        var cases = new (string Name, string Body, string Prefer)[]
                {
            ("objectrepresentation", row, "resolution=merge-duplicates,return=representation"),
        ("arrayrepresentation", "[" + row + "]", "resolution=merge-duplicates,return=representation"),
        ("objectminimal", row, "resolution=merge-duplicates,return=minimal"),
                }
        ;

        using var http = new HttpClient();
        foreach (var c in cases)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, $"{url}/rest/v1/positions?on_conflict=position_code");
                req.Headers.TryAddWithoutValidation("apikey", key);
                req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {key}");
                req.Headers.TryAddWithoutValidation("Prefer", c.Prefer);
                req.Content = new StringContent(c.Body, System.Text.Encoding.UTF8, "application/json");
                using var res = await http.SendAsync(req);
                var text = await res.Content.ReadAsStringAsync();
                Console.WriteLine(
                $"[PG-PROBE] case={c.Name} status={(int)res.StatusCode} version={res.Version} " +

                $"contentLength={req.Content.Headers.ContentLength} response={text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PG-PROBE] case={c.Name} exception={ex.GetType().Name}: {ex.Message}");
            }
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