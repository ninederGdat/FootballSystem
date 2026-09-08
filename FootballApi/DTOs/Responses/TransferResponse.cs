using System;
using System.ComponentModel.DataAnnotations;

namespace FootballApi.DTOs.Responses;

public sealed class TransferResponse
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = null!;

    public int FromClubId { get; init; }
    public string FromClubName { get; init; } = null!;

    public int ToClubId { get; init; }
    public string ToClubName { get; init; } = null!;

    public DateTime TransferDate { get; init; }

    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public string? TransferType { get; init; }

    public bool OnLoan { get; init; }
    public bool ContractExtension { get; init; }

    public decimal? Fee { get; init; }
}