using FotmobSync.Services;
using Xunit;

namespace FotmobSync.Tests.Services;

public class FormationNameTests
{
    [Theory]
    [InlineData("4-3-3")]
    [InlineData("4-2-3-1")]
    [InlineData("4-3-2-1")]
    [InlineData("3-4-1-2")]
    [InlineData("4-1-2-1-2")]
    public void IsValidFormationName_AcceptsFotmobFormations(string name)
    {
        Assert.True(FormationService.IsValidFormationName(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("433")]
    [InlineData("abc")]
    [InlineData("4--3")]
    [InlineData("10-0")]
    [InlineData("4-3-3-")]
    [InlineData("4-3-3-1-1-1-1")]
    public void IsValidFormationName_RejectsOtherText(string? name)
    {
        Assert.False(FormationService.IsValidFormationName(name));
    }
}