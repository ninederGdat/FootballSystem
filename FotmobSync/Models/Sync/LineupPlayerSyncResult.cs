public sealed record LineupPlayerSyncResult(int Starters, int Total)
{
    public bool IsComplete => Starters == 11;
}