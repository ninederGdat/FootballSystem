using System.Text.Json.Serialization;

namespace FotmobSync.Models.Raw
{
    /// <summary>
    /// Raw model từ FotMob fixtures (<c>fixtures.allFixtures.fixtures[]</c> trong team data).
    /// </summary>
    public class MatchRaw
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("home")]
        public MatchTeamSideRaw? Home { get; set; }

        [JsonPropertyName("away")]
        public MatchTeamSideRaw? Away { get; set; }

        [JsonPropertyName("tournament")]
        public MatchTournamentRaw? Tournament { get; set; }

        [JsonPropertyName("status")]
        public MatchStatusRaw? Status { get; set; }
    }

    public class MatchTeamSideRaw
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("score")]
        public int? Score { get; set; }
    }

    public class MatchTournamentRaw
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("leagueId")]
        public int LeagueId { get; set; }
    }

    public class MatchStatusRaw
    {
        [JsonPropertyName("utcTime")]
        public string? UtcTime { get; set; }

        [JsonPropertyName("started")]
        public bool Started { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonPropertyName("cancelled")]
        public bool Cancelled { get; set; }
    }
}
