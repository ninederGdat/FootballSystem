// Infrastructure/External/FotmobBrowserClient.cs
using FotmobSync.Models.Raw;
using Microsoft.Playwright;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FotmobSync.Infrastructure.External;

public class FotmobBrowserClient : IAsyncDisposable
{
    private readonly ILogger<FotmobBrowserClient> _logger;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FotmobBrowserClient(ILogger<FotmobBrowserClient> logger)
    {
        _logger = logger;
    }

    private async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_playwright != null) return;

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-blink-features=AutomationControlled", "--disable-dev-shm-usage" }
            });
        }
        finally { _semaphore.Release(); }
    }

    public async Task<PlayerRaw?> GetPlayerDetailAsync(long playerId)
    {
        await InitializeAsync();

        await using var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        var page = await context.NewPageAsync();

        try
        {
            var url = $"https://www.fotmob.com/players/{playerId}";
            _logger.LogInformation("🌐 Loading player page: {Url}", url);

            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

            await Task.Delay(5000); // Đợi hydrate __NEXT_DATA__

            var html = await page.ContentAsync();

            // Extract __NEXT_DATA__
            var match = Regex.Match(html,
                @"<script id=""__NEXT_DATA__"" type=""application/json"">(.*?)</script>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                _logger.LogWarning("Không tìm thấy __NEXT_DATA__ cho player {PlayerId}", playerId);
                return null;
            }

            var jsonContent = match.Groups[1].Value;
            var nextData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

            // Extract player data
            var playerJson = ExtractPlayerFromNextData(nextData, playerId);

            if (playerJson.HasValue)
            {
                var playerRaw = JsonSerializer.Deserialize<PlayerRaw>(playerJson.Value.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Successfully extracted player {PlayerId} from __NEXT_DATA__", playerId);
                return playerRaw;
            }

            _logger.LogWarning("Không extract được player data từ __NEXT_DATA__");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy player {PlayerId} qua __NEXT_DATA__", playerId);
            return null;
        }
    }

    private JsonElement? ExtractPlayerFromNextData(JsonElement nextData, long playerId)
    {
        try
        {
            // Path 1: props.pageProps.data
            if (nextData.TryGetProperty("props", out var props) &&
                props.TryGetProperty("pageProps", out var pageProps))
            {
                if (pageProps.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("id", out var id) &&
                    id.TryGetInt64(out var idValue) &&
                    idValue == playerId)
                {
                    return data;
                }

                // Path 2: fallback
                if (pageProps.TryGetProperty("fallback", out var fallback))
                {
                    var key = $"player:{playerId}";
                    if (fallback.TryGetProperty(key, out var playerData))
                        return playerData;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting player from NextData");
        }

        return null;
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
            _playwright?.Dispose();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}