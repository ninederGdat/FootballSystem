using FotmobSync.Models.Clean;

namespace FotmobSync.Services;

public interface IFotmobEtlService
{
    /// <summary>
    /// Đồng bộ thông tin đội bóng + toàn bộ squad (cầu thủ)
    /// </summary>
    Task SyncTeamAndSquadAsync(int teamId);

    /// <summary>
    /// Đồng bộ thông tin chi tiết của một cầu thủ từ Player Detail API
    /// </summary>
    Task SyncPlayerAsync(int playerId, long teamId);

    /// <summary>
    /// Trích xuất danh sách cầu thủ từ Team API (đã có sẵn)
    /// </summary>
    Task<List<PlayerClean>> ExtractSquadAsync(int teamId);
}