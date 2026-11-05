using System.Text.Json;
using FotmobSync.Mappers;
using FotmobSync.Models.Raw;
using Xunit;

namespace FotmobSync.Tests.Mappers;

public class MatchEventMapperTests
{
    [Theory]
    [InlineData("Goal", null, null, "goal")]
    [InlineData("Goal", true, null, "own_goal")]
    [InlineData("Card", null, "Yellow", "yellow")]
    [InlineData("Card", null, "YellowRed", "yellow_red")]
    [InlineData("Card", null, "Red", "red")]
    public void ToUpsert_SupportedEventType_ReturnsExpectedEventType(
        string type,
        bool? ownGoal,
        string? card,
        string expectedEventType)
    {
        var raw = CreateRaw(
            $$"""{"eventId":1,"type":"{{type}}","time":12,"isHome":true,"ownGoal":{{(ownGoal.HasValue ? ownGoal.Value.ToString().ToLowerInvariant() : "null")}},"card":{{(card is null ? "null" : JsonSerializer.Serialize(card))}}}""");

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.NotNull(result);
        Assert.Equal(expectedEventType, result.EventType);
    }

    [Theory]
    [InlineData("Substitution", null)]
    [InlineData("AddedTime", null)]
    [InlineData("Card", "Blue")]
    public void ToUpsert_UnsupportedEventType_ReturnsNull(string type, string? card)
    {
        var raw = CreateRaw(
            $$"""{"eventId":2,"type":"{{type}}","time":20,"isHome":false,"card":{{(card is null ? "null" : JsonSerializer.Serialize(card))}}}""");

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.Null(result);
    }

    [Theory]
    [InlineData(12, 3, 12, 3)]
    [InlineData(90, null, 90, null)]
    public void ToUpsert_TimeFields_MapsMinuteAndStoppageTime(
        int time,
        int? overloadTime,
        int expectedMinute,
        int? expectedStoppageTime)
    {
        var raw = CreateRaw(
            $$"""{"eventId":3,"type":"Goal","time":{{time}},"overloadTime":{{(overloadTime?.ToString() ?? "null")}},"isHome":true}""");

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.NotNull(result);
        Assert.Equal(expectedMinute, result.Minute);
        Assert.Equal(expectedStoppageTime, result.StoppageTime);
    }

    [Theory]
    [InlineData(true, 100L, 100L)]
    [InlineData(true, 200L, null)]
    [InlineData(false, 200L, 200L)]
    [InlineData(false, 100L, null)]
    public void ToUpsert_TeamId_IsSetOnlyWhenEventTeamIsFollowedTeam(
        bool isHome,
        long followedTeamId,
        long? expectedMappedTeamId)
    {
        var raw = CreateRaw(
            $$"""{"eventId":4,"type":"Goal","time":1,"isHome":{{isHome.ToString().ToLowerInvariant()}}}""");

        var result = raw.ToUpsert(55, 100, 200, followedTeamId, new HashSet<long>());

        Assert.NotNull(result);
        Assert.Equal(expectedMappedTeamId, result.TeamId);
    }

    [Theory]
    [InlineData(7L, 8L, 7L, 8L)]
    [InlineData(99L, 98L, null, null)]
    [InlineData(7L, 98L, 7L, null)]
    [InlineData(null, 8L, null, 8L)]
    public void ToUpsert_PlayerIds_OnlyMapsKnownLocalPlayers(
        long? playerId,
        long? assistPlayerId,
        long? expectedPlayerId,
        long? expectedAssistPlayerId)
    {
        var raw = CreateRaw(
            $$"""{"eventId":5,"type":"Goal","time":1,"isHome":true,"player":{{(playerId.HasValue ? $$"""{"id":{{playerId.Value}},"name":"Player"}""" : "null")}},"assistPlayerId":{{(assistPlayerId?.ToString() ?? "null")}}}""");
        var knownPlayerIds = new HashSet<long> { 7, 8 };

        var result = raw.ToUpsert(55, 100, 200, 100, knownPlayerIds);

        Assert.NotNull(result);
        Assert.Equal(expectedPlayerId, result.PlayerId);
        Assert.Equal(expectedAssistPlayerId, result.AssistPlayerId);
    }

    [Fact]
    public void ToUpsert_DescriptionAndRawPayload_PreservesSourceEvent()
    {
        const string json = """{"eventId":6,"type":"Goal","time":45,"isHome":true,"goalDescriptionKey":"penalty","extraField":{"value":true}}""";
        var raw = CreateRaw(json);

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.NotNull(result);
        Assert.Equal("penalty", result.DescriptionKey);
        Assert.Equal(json, result.RawPayload);
        Assert.Contains("\"extraField\":{\"value\":true}", result.RawPayload);
    }

    [Fact]
    public void ToUpsertList_EventsWithUnsupportedEntries_PreservesSourceArrayIndexes()
    {
        var rawEvents = new[]
        {
            CreateRaw("""{"eventId":10,"type":"Goal","time":1,"isHome":true}""", sourceIndex: 0),
            CreateRaw("""{"eventId":11,"type":"Substitution","time":2,"isHome":false}""", sourceIndex: 1),
            CreateRaw("""{"eventId":12,"type":"Card","card":"Red","time":3,"isHome":false}""", sourceIndex: 2),
        };

        var result = rawEvents.ToUpsertList(55, 100, 200, 100, new HashSet<long>());

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { 0, 2 }, result.Select(item => item.EventOrder));
    }

    [Fact]
    public void ToUpsert_NullPlayerAndMissingOptionalFields_MapsThemAsNull()
    {
        var raw = CreateRaw("""{"eventId":13,"type":"Goal","time":5,"isHome":true,"player":null}""");

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.NotNull(result);
        Assert.Null(result.PlayerId);
        Assert.Null(result.AssistPlayerId);
        Assert.Null(result.StoppageTime);
        Assert.Null(result.DescriptionKey);
    }

    [Fact]
    public void ToUpsert_PenaltyShootoutGoal_MapsAsGoalAndRetainsShootoutFlagInRawPayload()
    {
        const string json = """{"eventId":14,"type":"Goal","time":120,"isHome":true,"isPenaltyShootoutEvent":true}""";
        var raw = CreateRaw(json);

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.NotNull(result);
        // REVIEW: Shootout events are not distinguished in the clean event type; the current mapper emits a goal.
        Assert.Equal("goal", result.EventType);
        Assert.Equal(json, result.RawPayload);
    }

    [Fact]
    public void ToUpsert_NullRaw_ReturnsNull()
    {
        MatchEventRaw? raw = null;

        var result = raw.ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.Null(result);
    }

    private static MatchEventRaw CreateRaw(string json, int sourceIndex = 0)
    {
        using var document = JsonDocument.Parse(json);
        var raw = JsonSerializer.Deserialize<MatchEventRaw>(json)!;
        raw.RawJson = document.RootElement.Clone();
        raw.SourceIndex = sourceIndex;
        return raw;
    }
}
