using System.ComponentModel.DataAnnotations;

namespace FootballApi.DTOs.Transfers;

public sealed record TransferStatisticsQuery
{
    public string? Season { get; init; }

    [Range(1, long.MaxValue, ErrorMessage = "TeamId must be a positive integer.")]
    public long? TeamId { get; init; } = 8455;

    public bool? OnLoan { get; init; }
}

