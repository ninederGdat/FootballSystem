namespace FotmobSync.Services;

public static class SyncRunStatus
{
    public const string Running = "running";
    public const string Success = "success";
    public const string Partial = "partial";
    public const string Failed = "failed";

    public const int MaxErrorLength = 500;

    /// <summary>
    /// failed  = workflow did not complete (aborted, cancelled, or an exception escaped);
    /// partial = completed but at least one phase failed;
    /// success = completed and every phase succeeded.
    /// </summary>
    public static string Resolve(bool completed, bool anyPhaseFailed) =>
        !completed ? Failed : anyPhaseFailed ? Partial : Success;

    public static string? Truncate(string? message, int max = MaxErrorLength) =>
        message is null || message.Length <= max ? message : message[..max];
}