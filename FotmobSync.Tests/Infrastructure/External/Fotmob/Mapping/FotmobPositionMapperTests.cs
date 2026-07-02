using FotmobSync.Infrastructure.External.Fotmob.Mapping;
using Xunit;

namespace FotmobSync.Tests.Infrastructure.External.Fotmob.Mapping;

public class FotmobPositionMapperTests
{
    private readonly IFotmobPositionMapper _positionMapper = new FotmobPositionMapper();

    [Theory]
    [InlineData(11, "GK")]
    [InlineData(32, "RB")]
    [InlineData(34, "CB")]
    [InlineData(62, "RWB")]
    [InlineData(84, "AM")]
    [InlineData(115, "ST")]
    public void TryMap_KnownPositionIds_ReturnsExpectedCode(int positionId, string expectedCode)
    {
        string? positionCode = null;

        if (_positionMapper.TryMap(positionId, out var mapped))
        {
            positionCode = mapped;
        }

        Assert.NotNull(positionCode);
        Assert.Equal(expectedCode, positionCode);
    }
}
