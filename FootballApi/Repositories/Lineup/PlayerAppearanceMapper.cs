namespace FootballApi.Repositories.Lineup;

public static class PlayerAppearanceMapper
{
    public static PlayerAppearanceRecord ToRecord(
        this PlayerAppearanceViewClean row, int goals, int assists) => new()
        {
            MatchId = row.MatchId,
            MatchDate = row.MatchDate,
            OpponentName = row.OpponentName,
            CompetitionName = row.CompetitionName,
            PlayedForTeamId = row.PlayedForTeamId,
            IsStarter = row.IsStarter ?? true,
            PositionCode = row.PositionCode,
            RoleId = row.RoleId is { } r ? checked((int)r) : null,
            MinuteIn = row.MinuteIn,
            MinuteOut = row.MinuteOut,
            Goals = goals,
            Assists = assists
        };
}