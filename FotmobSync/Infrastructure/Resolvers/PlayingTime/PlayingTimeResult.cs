using FotmobSync.Infrastructure.Resolvers.PlayingTime;

public sealed record PlayingTimeResult(
    int? MinuteIn,
    int? MinuteOut);