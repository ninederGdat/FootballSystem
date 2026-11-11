using FootballSystem.Shared.Models.Clean;

namespace FootballApi.DTOs.Matches;

/// <summary>
/// Fotmob trả tỉ số cộng dồn đã gồm luân lưu (vd 1-1 rồi luân lưu 6-5 -> 7-6).
/// API công bố tỉ số hết giờ (đã trừ luân lưu) và điểm luân lưu riêng.
/// </summary>
public static class MatchScoreExtensions
{
    public static int? RegularScoreHome(this MatchClean match) =>
        match.ScoreHome - (match.PenaltyHome ?? 0);

    public static int? RegularScoreAway(this MatchClean match) =>
        match.ScoreAway - (match.PenaltyAway ?? 0);
}