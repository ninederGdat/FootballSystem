using System.Text.Json;
using FotmobSync.Mappers;
using FotmobSync.Models.Raw;
using Xunit;

namespace FotmobSync.Tests.Mappers;

public class PenaltyShootoutMapperTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // Mẫu thật từ Fotmob (Liverpool - Chelsea, EFL Cup 28/10/2026): event tóm tắt, không có cầu thủ.
    private const string RealSample = """
        {"type":"PenaltyShootout","card":null,"cardDescription":null,"cardReason":null,
         "missedPenaltyOutcome":null,"time":121,"addedTime":null,"overloadTime":null,
         "ownGoal":null,"goalDescription":null,"goalDescriptionKey":null,
         "penaltyScore":[6,5],"player":{"id":null,"profileUrl":""},
         "homeScore":7,"awayScore":6,"swap":[{},{}],"VAR":null}
        """;

    [Fact]
    public void TryGetScore_RealFotmobSample_ReturnsHomeAndAwayPenaltyScore()
    {
        var score = new[] { Parse(RealSample) }.TryGetScore();

        Assert.NotNull(score);
        Assert.Equal(6, score.Value.Home);
        Assert.Equal(5, score.Value.Away);
    }

    [Fact]
    public void TryGetScore_NoShootoutEvent_ReturnsNull()
    {
        var events = new[]
        {
            Parse("""{"eventId":1,"type":"Goal","time":10,"isHome":true}"""),
            Parse("""{"eventId":2,"type":"Card","time":20,"isHome":false,"card":"Yellow"}""")
        };

        Assert.Null(events.TryGetScore());
    }

    [Fact]
    public void TryGetScore_ShootoutEventWithoutPenaltyScore_ReturnsNull()
    {
        var events = new[] { Parse("""{"type":"PenaltyShootout","time":121}""") };

        Assert.Null(events.TryGetScore());
    }

    [Theory]
    [InlineData("[6]")]
    [InlineData("[6,5,4]")]
    [InlineData("[-1,5]")]
    public void TryGetScore_InvalidPenaltyScore_ReturnsNull(string penaltyScore)
    {
        var events = new[]
        {
            Parse($$"""{"type":"PenaltyShootout","penaltyScore":{{penaltyScore}},"time":121}""")
        };

        Assert.Null(events.TryGetScore());
    }

    [Fact]
    public void TryGetScore_MultipleShootoutEvents_UsesTheLastOne()
    {
        var events = new[]
        {
            Parse("""{"type":"PenaltyShootout","time":121,"penaltyScore":[1,0]}"""),
            Parse("""{"type":"PenaltyShootout","time":121,"penaltyScore":[6,5]}""")
        };

        var score = events.TryGetScore();

        Assert.NotNull(score);
        Assert.Equal(6, score.Value.Home);
        Assert.Equal(5, score.Value.Away);
    }

    [Fact]
    public void TryGetScore_GoalFlaggedAsShootout_IsIgnored()
    {
        var events = new[]
        {
            Parse("""{"eventId":3,"type":"Goal","time":120,"isHome":true,"isPenaltyShootoutEvent":true,"penaltyScore":[3,2]}""")
        };

        Assert.Null(events.TryGetScore());
    }

    [Fact]
    public void ToUpsert_PenaltyShootoutEvent_IsNotStoredAsMatchEventRow()
    {
        var result = Parse(RealSample).ToUpsert(55, 100, 200, 100, new HashSet<long>());

        Assert.Null(result);
    }

    private static MatchEventRaw Parse(string json) =>
        JsonSerializer.Deserialize<MatchEventRaw>(json, JsonOptions)!;
}