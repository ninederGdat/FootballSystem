using FootballApi.Common.Exceptions;
using FootballApi.Configuration;
using FootballApi.Services.Season;
using Xunit;

namespace FotmobSync.Tests.Season;

public class SeasonResolverTests
{
    [Fact]
    public void Resolve_KnownCode_ReturnsConfiguredSeason()
    {
        var options = new SeasonOptions
        {
            Seasons =
            [
                new SeasonDefinition("2025-26", "2025/26", new DateTime(2025, 8, 1), new DateTime(2026, 8, 1)),
                new SeasonDefinition("2026-27", "2026/27", new DateTime(2026, 8, 1), new DateTime(2027, 8, 1))
            ]
        };

        var resolver = new SeasonResolver(options);

        var season = resolver.Resolve("2025-26");

        Assert.Equal("2025-26", season.Code);
        Assert.Equal("2025/26", season.Name);
        Assert.Equal(new DateTime(2025, 8, 1), season.StartDate);
        Assert.Equal(new DateTime(2026, 8, 1), season.EndDate);
    }

    [Fact]
    public void Resolve_UnknownCode_ThrowsBadRequestException()
    {
        var options = new SeasonOptions
        {
            Seasons =
            [
                new SeasonDefinition("2025-26", "2025/26", new DateTime(2025, 8, 1), new DateTime(2026, 8, 1))
            ]
        };

        var resolver = new SeasonResolver(options);

        var exception = Assert.Throws<BadRequestException>(() => resolver.Resolve("does-not-exist"));

        Assert.Contains("does-not-exist", exception.Message);
    }
}
