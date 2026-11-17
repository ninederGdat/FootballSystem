using FotmobSync.Services;
using Xunit;

namespace FotmobSync.Tests.SyncRuns;

public class SyncRunStatusTests
{
    [Theory]
    [InlineData(true, false, "success")]
    [InlineData(true, true, "partial")]
    [InlineData(false, false, "failed")]
    [InlineData(false, true, "failed")]
    public void Resolve_MapsCompletionAndPhaseFailures(bool completed, bool anyFailed, string expected)
    {
        Assert.Equal(expected, SyncRunStatus.Resolve(completed, anyFailed));
    }

    [Fact]
    public void Truncate_Null_ReturnsNull() => Assert.Null(SyncRunStatus.Truncate(null));

    [Fact]
    public void Truncate_ShortMessage_IsUnchanged() =>
        Assert.Equal("boom", SyncRunStatus.Truncate("boom"));

    [Fact]
    public void Truncate_LongMessage_IsCutToLimit()
    {
        var result = SyncRunStatus.Truncate(new string('x', 800));

        Assert.Equal(SyncRunStatus.MaxErrorLength, result!.Length);
    }
}