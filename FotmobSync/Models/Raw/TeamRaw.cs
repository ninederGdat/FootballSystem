using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FotmobSync.Models.Raw
{
    /// <summary>
    /// Raw model từ FotMob Team API (/api/data/teams?id=...)
    /// </summary>
    public class TeamRaw
    {
        [JsonPropertyName("details")]
        public TeamDetailsRaw? Details { get; set; }

        [JsonPropertyName("squad")]
        public SquadContainerRaw? Squad { get; set; }

    }

    // Thông tin cơ bản của đội
    public class TeamDetailsRaw
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("shortName")]
        public string? ShortName { get; set; }

        [JsonPropertyName("logo")]
        public string? LogoUrl { get; set; }

    }

    // Container cho squad
    public class SquadContainerRaw
    {
        [JsonPropertyName("squad")]
        public List<SquadGroupRaw> Groups { get; set; } = new();
    }

    public class SquadGroupRaw
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }                     // "coach", "keepers", "defenders", ...

        [JsonPropertyName("members")]
        public List<SquadMemberRaw> Members { get; set; } = new();
    }

    // Member dùng chung cho cả HLV và cầu thủ trong squad
    public class SquadMemberRaw
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("shirtNumber")]
        public JsonElement? ShirtNumber { get; set; }         // có thể là null hoặc number/string

        [JsonPropertyName("ccode")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("cname")]
        public string? CountryName { get; set; }
    }
}
