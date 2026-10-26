using System.Text.Json;
using System.Text.Json.Serialization;
using FootballSystem.Shared.Players;
using Xunit;

namespace FotmobSync.Tests.Players;

public class PlayerRelationExtensionsTests
{
    [Theory]
    [InlineData(PlayerRelation.Current)]
    [InlineData(PlayerRelation.LoanedIn)]
    [InlineData(PlayerRelation.LoanedOut)]
    [InlineData(PlayerRelation.Transferred)]
    public void SerializedValue_CanBeParsedBack(PlayerRelation relation)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));

        var json = JsonSerializer.Serialize(relation, options);   // ví dụ "\"loaned_in\""
        var text = json.Trim('"');

        Assert.True(PlayerRelationExtensions.TryParse(text, out var parsed));
        Assert.Equal(relation, parsed);
    }
}
