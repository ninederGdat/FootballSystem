namespace FootballSystem.Shared.Players;

public enum PlayerRelation
{
    Current,     // thuộc CLB đang xem (mua đứt cũng tính là current)
    LoanedIn,    // đang mượn về CLB đang xem
    LoanedOut,   // CLB đang xem cho mượn đi
    Transferred  // đã rời CLB đang xem
}