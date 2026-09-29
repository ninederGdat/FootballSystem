using System.ComponentModel.DataAnnotations;
namespace FootballApi.DTOs.Statistics;


/// <summary> /// DTO chứa thông tin truy vấn thống kê chuyển nhượng. /// </summary>
public sealed record TransferStatisticsQuery
{
    /// <summary>
    /// Chuỗi biểu diễn mùa giải dạng YYYY-YYYY (ví dụ: "2026-2027").
    /// </summary>
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Tham số season phải có định dạng YYYY-YYYY (ví dụ: 2026-2027).")]
    public string? Season { get; init; }

    /// <summary>
    /// ID đội bóng cần truy vấn thống kê (Mặc định: 8455 - Chelsea FC).
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "TeamId phải là số nguyên dương hợp lệ.")]
    public long? TeamId { get; init; } = 8455;

    /// <summary>
    /// Bộ lọc giao dịch mượn: true (chỉ mượn), false (chỉ mua/bán đứt), null (tất cả).
    /// </summary>
    public bool? OnLoan { get; init; }
}

