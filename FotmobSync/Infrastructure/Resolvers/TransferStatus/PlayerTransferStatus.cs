namespace FotmobSync.Infrastructure.Resolvers.TransferStatus;

public class PlayerTransferStatus
{
    public long PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public long? FromClubId { get; set; }
    public long? ToClubId { get; set; }
    public string status { get; set; } = "current"; // "current" | "loaned" | "transferred"
}