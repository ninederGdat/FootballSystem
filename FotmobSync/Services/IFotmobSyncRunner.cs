namespace FotmobSync.Services;

public interface IFotmobSyncRunner
{
    Task RunAsync(CancellationToken cancellationToken = default);
}