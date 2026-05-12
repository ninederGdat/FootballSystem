using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FotmobSync.Models.Raw
{
    /// <summary>
    /// Raw model  Player Detail API
    /// </summary>
    public class PlayerRaw
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("birthDate")]
        public BirthDateRaw? BirthDate { get; set; }

        [JsonPropertyName("contractEnd")]
        public ContractEndRaw? ContractEnd { get; set; }

        [JsonPropertyName("marketValues")]
        public MarketValuesRaw? MarketValues { get; set; }     // ← Đã sửa

        [JsonPropertyName("positionDescription")]
        public PositionDescriptionRaw? PositionDescription { get; set; }

        [JsonPropertyName("primaryTeam")]
        public PrimaryTeamRaw? PrimaryTeam { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("injuryInformation")]
        public InjuryInformationRaw? InjuryInformation { get; set; }

        [JsonPropertyName("playerInformation")]
        public List<PlayerInformationRaw>? PlayerInformation { get; set; }

        [JsonPropertyName("ccode")]
        public string? CountryCode { get; set; }

        // Thêm để lấy nationality từ meta nếu cần
        [JsonPropertyName("meta")]
        public MetaRaw? Meta { get; set; }
    }
    // Các class con hỗ trợ parse
    public class BirthDateRaw
    {
        [JsonPropertyName("utcTime")]
        public string? UtcTime { get; set; }
    }

    public class ContractEndRaw
    {
        [JsonPropertyName("utcTime")]
        public string? UtcTime { get; set; }
    }

    public class PositionDescriptionRaw
    {
        [JsonPropertyName("primaryPosition")]
        public PrimaryPositionRaw? PrimaryPosition { get; set; }

        [JsonPropertyName("nonPrimaryPositions")]
        public List<NonPrimaryPositionRaw>? NonPrimaryPositions { get; set; }

        [JsonPropertyName("positions")]
        public List<PositionOccurrenceRaw>? Positions { get; set; }   // Thêm để linh hoạt
    }

    public class PrimaryPositionRaw
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }
    }

    public class NonPrimaryPositionRaw
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }
    }

    public class PositionOccurrenceRaw
    {
        [JsonPropertyName("strPos")]
        public PrimaryPositionRaw? StrPos { get; set; }
    }

    public class PrimaryTeamRaw
    {
        [JsonPropertyName("teamId")]
        public int TeamId { get; set; }
    }

    /// <summary>Fotmob player injury payload when the player is injured.</summary>
    public class InjuryInformationRaw
    {
        [JsonPropertyName("key")]
        public JsonElement? Key { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>ISO date string and/or <c>{ "utcTime": "..." }</c> (Fotmob date shapes).</summary>
        [JsonPropertyName("expectedReturn")]
        public JsonElement? ExpectedReturn { get; set; }
    }

    public class PlayerInformationRaw
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("value")]
        public PlayerInfoValueRaw? Value { get; set; }
    }

    public class PlayerInfoValueRaw
    {
        [JsonPropertyName("numberValue")]
        public int? NumberValue { get; set; }
    }

    // ==================== MARKET VALUES ====================
    public class MarketValuesRaw
    {
        [JsonPropertyName("values")]
        public List<MarketValueItemRaw>? Values { get; set; }
    }

    public class MarketValueItemRaw
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("value")]
        public decimal? Value { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }

    // ==================== META (cho nationality) ====================
    public class MetaRaw
    {
        [JsonPropertyName("personJSONLD")]
        public PersonJSONLDRaw? PersonJSONLD { get; set; }
    }

    public class PersonJSONLDRaw
    {
        [JsonPropertyName("nationality")]
        public NationalityRaw? Nationality { get; set; }
    }

    public class NationalityRaw
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
