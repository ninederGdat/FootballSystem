namespace FootballSystem.Shared.Players;

public static class PlayerRelationResolver
{
    public const string LoanedStatus = "loaned";

    /// <summary>
    /// Quan hệ của cầu thủ so với CLB đang xem.
    /// Trả null nếu cầu thủ không liên quan gì tới CLB đó.
    /// </summary>
    public static PlayerRelation? Resolve(
        long? contextTeamId,
        long teamId,
        long? currentTeamId,
        string? transferStatus)
    {
        var actualTeamId = currentTeamId ?? teamId;
        var isLoaned = string.Equals(transferStatus, LoanedStatus, StringComparison.OrdinalIgnoreCase);

        // Chế độ "Tất cả": một dòng mỗi cầu thủ, tính theo CLB chủ quản
        if (contextTeamId is null)
        {
            if (isLoaned && actualTeamId != teamId) return PlayerRelation.LoanedOut;
            if (actualTeamId != teamId) return PlayerRelation.Transferred;
            return PlayerRelation.Current;
        }

        var x = contextTeamId.Value;

        if (isLoaned)
        {
            if (teamId == x && actualTeamId != x) return PlayerRelation.LoanedOut;
            if (actualTeamId == x && teamId != x) return PlayerRelation.LoanedIn;
        }

        // Không mượn: mua đứt vào CLB đang xem cũng là Current
        if (actualTeamId == x) return PlayerRelation.Current;

        // Từng thuộc CLB đang xem nhưng giờ đã chuyển đi hẳn
        if (teamId == x) return PlayerRelation.Transferred;

        return null;
    }

    public static bool IsOnLoan(long teamId, long? currentTeamId, string? transferStatus) =>
    string.Equals(transferStatus, LoanedStatus, StringComparison.OrdinalIgnoreCase)
    && (currentTeamId ?? teamId) != teamId;
}