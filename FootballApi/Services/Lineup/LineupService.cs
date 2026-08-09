using FootballApi.Common.Exceptions;
using FootballApi.DTOs.Responses;
using FootballApi.Repositories.Position;
using FootballApi.Repositories.PositionRole;
using FootballSystem.Shared.Models.Clean;

public class LineupService : ILineupService
{
    private readonly ILineupRepository _repo;
    private readonly IPositionRepository _positionRepository;
    private readonly IPositionRoleRepository _positionRoleRepository;

    public LineupService(
        ILineupRepository repo,
        IPositionRepository positionRepository,
        IPositionRoleRepository positionRoleRepository)
    {
        _repo = repo;
        _positionRepository = positionRepository;
        _positionRoleRepository = positionRoleRepository;
    }


    public async Task<LineupResponse> GetLineupByMatchIdAsync(long matchId)
    {
        var lineup = await _repo.GetLineupByMatchIdAsync(matchId)
            ?? throw new NotFoundException($"Lineup not found for match {matchId}");

        return await BuildLineupResponseAsync(lineup);
    }

    public async Task<LineupResponse?> GetLineupByMatchIdOrDefaultAsync(long matchId)
    {
        var lineup = await _repo.GetLineupByMatchIdAsync(matchId);
        if (lineup == null)
            return null;

        return await BuildLineupResponseAsync(lineup);
    }


    private async Task<LineupResponse> BuildLineupResponseAsync(LineupClean lineup)
    {
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
        var positionsTask = _positionRepository.GetByCodesAsync(positionCodes);
        var rolesTask = _positionRoleRepository.GetByIdsAsync(roleIds);

        // Đợi toàn bộ query hoàn thành
        await Task.WhenAll(formationTask, playersTask, positionsTask, rolesTask);

        // Chuyển List thành Dictionary để tra cứu O(1) khi mapping
        var playerNameMap = playersTask.Result.ToDictionary(p => p.PlayerId, p => p.Name);
        var positionMap = positionsTask.Result.ToDictionary(p => p.PositionCode, p => p);
        var roleMap = rolesTask.Result.ToDictionary(r => r.Id, r => r);

        // Local Function: chuyển LineupPlayerClean -> LineupPlayerResponse
        LineupPlayerResponse MapSingle(LineupPlayerClean p)
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
        }

        var starters = lineupPlayers.Where(p => p.IsStarter)
            .Select(MapSingle)
            .OrderBy(r => r.CustomX ?? 0.5)
            .ThenBy(r => r.CustomY ?? 0.5)
            .ToList();

        var substitutes = lineupPlayers.Where(p => !p.IsStarter)
            .Select(MapSingle)
            .OrderBy(r => r.ShirtNumber ?? int.MaxValue)
            .ToList();

        return new LineupResponse
        {
            LineupId = lineup.Id,
            Type = lineup.Type,
            Formation = formationTask.Result is { } f
                ? new FormationDto { Id = f.Id, Name = f.Name, Description = f.Description }
                : null,
            Starters = starters,
            Substitutes = substitutes

        };
    }
}