using System;
using System.ComponentModel.DataAnnotations;

namespace FootballApi.DTOs.Transfers;

public sealed class TransferQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; init; } = 1;
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;
    public string? PlayerName { get; init; }
    public long? PlayerId { get; init; }
    public long? FromClubId { get; init; }
    public long? ToClubId { get; init; }
    public bool? OnLoan { get; init; }
    public bool? ContractExtension { get; init; }
    public string? TransferType { get; init; }
    public string? Season { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}