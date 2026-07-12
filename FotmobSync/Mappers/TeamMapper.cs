using FootballSystem.Shared.Models.Clean;
using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;

namespace FotmobSync.Mappers;

/// <summary>
/// Mapper chuyển đổi từ TeamRaw (JSON từ FotMob) sang TeamClean (model dùng cho Database)
/// </summary>
public static class TeamMapper
{
    /// <summary>
    /// Chuyển đổi TeamRaw sang TeamClean
    /// </summary>
    public static TeamClean ToClean(this TeamRaw? raw)
    {
        if (raw == null || raw.Details == null)
        {
            return new TeamClean();
        }

        var clean = new TeamClean
        {
            TeamId = raw.Details.Id,
            Name = raw.Details.Name?.Trim() ?? string.Empty,
            LogoUrl = raw.Details.LogoUrl?.Trim(),
            //LastUpdated = DateTime.UtcNow
        };

        // Xử lý thông tin HLV từ squad
        var coach = GetCoachFromSquad(raw.Squad);
        if (coach != null)
        {
            clean.CoachName = coach.Name?.Trim();
            clean.CoachNationality = coach.CountryName?.Trim() ?? coach.CountryCode?.Trim();
        }

        return clean;
    }

    /// <summary>
    /// Tìm HLV trong squad (group có title = "coach")
    /// </summary>
    private static SquadMemberRaw? GetCoachFromSquad(SquadContainerRaw? squadContainer)
    {
        if (squadContainer?.Groups == null || squadContainer.Groups.Count == 0)
            return null;

        // Tìm group có title là "coach" (không phân biệt hoa thường)
        var coachGroup = squadContainer.Groups.FirstOrDefault(g =>
            g.Title?.Trim().Equals("coach", StringComparison.OrdinalIgnoreCase) == true);

        // Lấy thành viên đầu tiên trong group coach (thường chỉ có 1 HLV chính)
        return coachGroup?.Members?.FirstOrDefault();
    }

    /// <summary>
    /// Chuyển đổi danh sách TeamRaw sang danh sách TeamClean
    /// </summary>
    public static List<TeamClean> ToCleanList(this IEnumerable<TeamRaw> rawList)
    {
        return rawList.Select(raw => raw.ToClean()).ToList();
    }
}