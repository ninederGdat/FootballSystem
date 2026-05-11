using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FotmobSync.Clients;

public class FotmobClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FotmobClient> _logger;

    public FotmobClient(HttpClient httpClient, ILogger<FotmobClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        ConfigureHeaders();
    }

    private void ConfigureHeaders()
    {
        // User-Agent giống request thật 
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0");

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");

        _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.fotmob.com");
        _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.fotmob.com/vi/players/807729/reece-james");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
    }

    /// <summary>
    /// Cập nhật x-mas token 
    /// </summary>
    public void SetXMasToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Remove("x-mas");
        _httpClient.DefaultRequestHeaders.Add("x-mas", token);
        _logger.LogInformation("✅ x-mas token đã được áp dụng.");
    }

    public async Task<JsonDocument> GetTeamDataAsync(int teamId)
    {
        var url = $"https://www.fotmob.com/api/data/teams?id={teamId}&ccode3=VNM";
        return await GetJsonAsync(url, $"Team {teamId}");
    }

    public async Task<JsonDocument> GetPlayerDataAsync(int playerId)
    {
        var url = $"https://www.fotmob.com/api/data/playerData?id={playerId}";
        return await GetJsonAsync(url, $"Player {playerId}");
    }

    private async Task<JsonDocument> GetJsonAsync(string url, string context)
    {
        try
        {
            _logger.LogInformation("Fetching: {Context}", context);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("FotMob returned {Status} for {Context}. Content: {Content}",
                    response.StatusCode, context, content);
            }

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching {Context}", context);
            throw;
        }
    }
}