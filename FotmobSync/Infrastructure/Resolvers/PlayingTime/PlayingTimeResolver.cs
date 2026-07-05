using FotmobSync.Infrastructure.Resolvers.PlayingTime;
using FotmobSync.Models.Raw;

public sealed class PlayingTimeResolver : IPlayingTimeResolver
{
    public PlayingTimeResult Resolve(
        LineupPlayerRaw player,
        bool isStarter)
    {
        int? minuteIn = isStarter ? 0 : null;
        int? minuteOut = null;

        var events = player.Performance?.SubstitutionEvents;

        if (events == null || events.Count == 0)
        {
            return new PlayingTimeResult(minuteIn, minuteOut);
        }

        foreach (var e in events)
        {
            switch (e.Type)
            {
                case "subIn":
                    minuteIn = e.Time;
                    break;

                case "subOut":
                    minuteOut = e.Time;
                    break;
            }
        }

        return new PlayingTimeResult(minuteIn, minuteOut);
    }
}