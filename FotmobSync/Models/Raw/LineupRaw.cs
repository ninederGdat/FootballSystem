using System.Text.Json.Serialization;

namespace FotmobSync.Models.Raw;

/// <summary>
/// Raw model từ FotMob lineup API (/api/data/lineups?id=...)
/// </summary>
public  class MatchLineupRaw
{
    [JsonPropertyName("matchId")]
    public long? MatchId { get; set; }

    [JsonPropertyName("lineupType")]
    public string? LineupType { get; set; }

    [JsonPropertyName("homeTeam")]
    public MatchLineupTeamRaw? HomeTeam { get; set; }

    [JsonPropertyName("awayTeam")]
    public MatchLineupTeamRaw? AwayTeam { get; set; }
}

public  class MatchLineupTeamRaw
{
    [JsonPropertyName("id")]
    public long TeamId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("formation")]
    public string? Formation { get; set; }

    [JsonPropertyName("starters")]
    public List<LineupPlayerRaw> Starters { get; set; } = [];

    [JsonPropertyName("subs")]
    public List<LineupPlayerRaw> Subs { get; set; } = [];

    [JsonPropertyName("unavailable")]
    public List<UnavailablePlayerRaw> Unavailable { get; set; } = [];

    [JsonPropertyName("coach")]
    public CoachRaw? Coach { get; set; }
}

public  class LineupPlayerRaw
{
    [JsonPropertyName("id")]
    public long PlayerId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("shirtNumber")]
    public string? ShirtNumber { get; set; }

    [JsonPropertyName("positionId")]
    public int? PositionId { get; set; }

    [JsonPropertyName("usualPlayingPositionId")]
    public int? UsualPlayingPositionId { get; set; }

    [JsonPropertyName("countryName")]
    public string? CountryName { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("horizontalLayout")]
    public LayoutPointRaw? HorizontalLayout { get; set; }

    [JsonPropertyName("verticalLayout")]
    public LayoutPointRaw? VerticalLayout { get; set; }

    [JsonPropertyName("marketValue")]
    public long? MarketValue { get; set; }

    [JsonPropertyName("performance")]
    public PlayerPerformanceRaw? Performance { get; set; }
}

public  class LayoutPointRaw
{
    [JsonPropertyName("x")]
    public double? X { get; set; }

    [JsonPropertyName("y")]
    public double? Y { get; set; }
}

public  class PlayerPerformanceRaw
{
    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("fantasyScore")]
    public string? FantasyScore { get; set; }

    [JsonPropertyName("playerOfTheMatch")]
    public bool? PlayerOfTheMatch { get; set; }

    [JsonPropertyName("substitutionEvents")]
    public List<SubstitutionEventRaw> SubstitutionEvents { get; set; } = [];

    [JsonPropertyName("events")]
    public List<PlayerEventRaw> Events { get; set; } = [];
}

public  class SubstitutionEventRaw
{
    [JsonPropertyName("time")]
    public int? Time { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public  class PlayerEventRaw
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public  class UnavailablePlayerRaw
{
    [JsonPropertyName("id")]
    public long PlayerId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("unavailability")]
    public UnavailabilityRaw? Unavailability { get; set; }
}

public  class UnavailabilityRaw
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("expectedReturn")]
    public string? ExpectedReturn { get; set; }
}

public  class CoachRaw
{
    [JsonPropertyName("id")]
    public long CoachId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}