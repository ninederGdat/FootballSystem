namespace FotmobSync.Models.Sync;

public record SquadPlayerRef(
    long PlayerId,
    long TeamId,
    string Name);