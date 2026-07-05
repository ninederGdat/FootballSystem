using FotmobSync.Models.Raw;

namespace FotmobSync.Infrastructure.Resolvers.PlayingTime;

public interface IPlayingTimeResolver
{
    PlayingTimeResult Resolve(
        LineupPlayerRaw player,
        bool isStarter);
}