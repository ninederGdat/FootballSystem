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

        [JsonPropertyName("transfers")]
        public TransferContainerRaw? Transfers { get; set; }
    }

    // Container cho transfers, giống pattern SquadContainerRaw
    public class TransferContainerRaw
    {
        [JsonPropertyName("allTransfers")]
        public List<TransferRaw> AllTransfers { get; set; } = new();
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

    public class TransferRaw
    {
        [JsonPropertyName("playerId")]
        public long PlayerId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("transferDate")]
        public string TransferDate { get; set; } = string.Empty; // parse ở Mapper

        [JsonPropertyName("fromClub")]
        public string FromClub { get; set; } = string.Empty;

        [JsonPropertyName("fromClubFullName")]
        public string FromClubFullName { get; set; } = string.Empty;

        [JsonPropertyName("fromClubId")]
        public long FromClubId { get; set; }

        [JsonPropertyName("toClub")]
        public string ToClub { get; set; } = string.Empty;

        [JsonPropertyName("toClubFullName")]
        public string ToClubFullName { get; set; } = string.Empty;

        [JsonPropertyName("toClubId")]
        public long ToClubId { get; set; }

        [JsonPropertyName("fee")]
        public TransferFeeRaw? Fee { get; set; }

        [JsonPropertyName("amountEuroEstimated")]
        public decimal? AmountEuroEstimated { get; set; }

        [JsonPropertyName("transferType")]
        public TransferTypeRaw TransferType { get; set; } = new();

        [JsonPropertyName("contractExtension")]
        public bool ContractExtension { get; set; }

        [JsonPropertyName("onLoan")]
        public bool OnLoan { get; set; }

        [JsonPropertyName("fromDate")]
        public string? FromDate { get; set; }

        [JsonPropertyName("toDate")]
        public string? ToDate { get; set; }

        [JsonPropertyName("marketValue")]
        public decimal? MarketValue { get; set; }
    }

    public class TransferFeeRaw
    {
        [JsonPropertyName("feeText")]
        public string? FeeText { get; set; }

        [JsonPropertyName("localizedFeeText")]
        public string? LocalizedFeeText { get; set; }

        [JsonPropertyName("value")]
        public decimal? Value { get; set; }
    }

    public class TransferTypeRaw
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("localizationKey")]
        public string LocalizationKey { get; set; } = string.Empty;
    }
}
