using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Responses;
using FootballSystem.Shared.Models.Clean;

public class LineupService : ILineupService
{
    private readonly ILineupRepository _repo;

    public LineupService(ILineupRepository repo) => _repo = repo;

    public async Task<LineupResponse> GetLineupByMatchIdAsync(long matchId)
    {
        // Guard clause: dừng ngay nếu trận đấu chưa có lineup
        var lineup = await _repo.GetLineupByMatchIdAsync(matchId)
            ?? throw new NotFoundException($"Lineup not found for match {matchId}");

        // Khởi tạo Task để có thể await đồng thời với các query khác.
        // Nếu không có Formation thì dùng Completed Task thay vì phải if ở dưới.
        var formationTask = lineup.FormationId.HasValue
            ? _repo.GetFormationByIdAsync(lineup.FormationId.Value)
            : Task.FromResult<FormationClean?>(null);

        // Lấy toàn bộ cầu thủ thuộc lineup
        var lineupPlayers = await _repo.GetLineupPlayersAsync(lineup.Id);

        // Chuẩn bị các khóa tra cứu, tránh query lặp (N+1 Query)
        var playerIds = lineupPlayers.Select(p => p.PlayerId).Distinct().ToList();
        var positionCodes = lineupPlayers
            .Where(p => p.PositionCode != null)
            .Select(p => p.PositionCode!)
            .Distinct()
            .ToList();
        var roleIds = lineupPlayers
            .Where(p => p.RoleId.HasValue)
            .Select(p => (int)p.RoleId!.Value)
            .Distinct()
            .ToList();

        // Khởi tạo các query độc lập để chạy song song
        var playersTask = _repo.GetPlayersByIdsAsync(playerIds);
        var positionsTask = _repo.GetPositionsByCodesAsync(positionCodes);
        var rolesTask = _repo.GetPositionRolesByIdsAsync(roleIds);

        // Đợi toàn bộ query hoàn thành
        await Task.WhenAll(formationTask, playersTask, positionsTask, rolesTask);

        // Chuyển List thành Dictionary để tra cứu O(1) khi mapping
        var playerNameMap = playersTask.Result.ToDictionary(p => p.PlayerId, p => p.Name);
        var positionMap = positionsTask.Result.ToDictionary(p => p.PositionCode, p => p);
        var roleMap = rolesTask.Result.ToDictionary(r => r.Id, r => r);

        // Local Function: chuyển LineupPlayerClean -> LineupPlayerResponse
        List<LineupPlayerResponse> Map(IEnumerable<LineupPlayerClean> players)
        {
            return players.Select(p =>
            {
                positionMap.TryGetValue(p.PositionCode ?? "", out var position);
                PositionRoleClean? role = null;
                if (p.RoleId.HasValue)
                    roleMap.TryGetValue((int)p.RoleId.Value, out role);

                return new LineupPlayerResponse
                {
                    PlayerId = p.PlayerId,
                    PlayerName = playerNameMap.GetValueOrDefault(p.PlayerId, "Unknown"),
                    PositionCode = p.PositionCode,
                    PositionName = position?.PositionName,
                    RoleId = p.RoleId,
                    RoleName = role?.RoleName,
                    RoleShort = role?.RoleShort,
                    ShirtNumber = p.ShirtNumber,
                    MinuteIn = p.MinuteIn,
                    MinuteOut = p.MinuteOut,
                    CustomX = p.CustomX,
                    CustomY = p.CustomY
                };
            })
            // Sort theo x_coord trước (GK ~0, tăng dần theo tuyến), rồi y_coord (trái -> phải trên cùng tuyến)
            .OrderBy(r => r.CustomX ?? 0.5)
            .ThenBy(r => r.CustomY ?? 0.5)
            .ToList();
        }

        return new LineupResponse
        {
            LineupId = lineup.Id,
            Type = lineup.Type,
            Formation = formationTask.Result is { } f
                ? new FormationDto { Id = f.Id, Name = f.Name, Description = f.Description }
                : null,
            Starters = Map(lineupPlayers.Where(p => p.IsStarter)),
            Substitutes = Map(lineupPlayers.Where(p => !p.IsStarter))
        };
    }
}