namespace FootballSystem.Shared.Players;

public static class PlayerRelationExtensions
{
    public static bool TryParse(string? value, out PlayerRelation relation)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "current": relation = PlayerRelation.Current; return true;
            case "loaned_in": relation = PlayerRelation.LoanedIn; return true;
            case "loaned_out": relation = PlayerRelation.LoanedOut; return true;
            case "transferred": relation = PlayerRelation.Transferred; return true;
            default: relation = default; return false;
        }
    }
}