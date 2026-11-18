namespace FootballApi.DTOs.Sync;

public sealed class SyncRunListResponse
{
    public IReadOnlyList<SyncRunResponse> Data { get; set; } = [];
}