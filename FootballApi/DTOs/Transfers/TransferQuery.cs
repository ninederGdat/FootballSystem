using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FootballApi.DTOs.Transfers;

public sealed class TransferQuery
{
    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;

    [FromQuery(Name = "playerName")]
    public string? PlayerName { get; init; }

    [FromQuery(Name = "playerId")]
    public long? PlayerId { get; init; }

    [FromQuery(Name = "fromClubId")]
    public long? FromClubId { get; init; }

    [FromQuery(Name = "toClubId")]
    public long? ToClubId { get; init; }

    [FromQuery(Name = "onLoan")]
    public bool? OnLoan { get; init; }

    [FromQuery(Name = "contractExtension")]
    public bool? ContractExtension { get; init; }

    [FromQuery(Name = "transferType")]
    public string? TransferType { get; init; }

    /// <summary>CLB theo dõi cần xem (from_club_id HOẶC to_club_id). Không phải CLB theo dõi → 400.</summary>
    [FromQuery(Name = "teamId")]
    [Range(1, long.MaxValue, ErrorMessage = "TeamId must be a positive integer.")]
    public long? TeamId { get; init; }

    /// <summary><c>in</c> (to_club_id = teamId) hoặc <c>out</c> (from_club_id = teamId). Bắt buộc có teamId.</summary>
    [FromQuery(Name = "direction")]
    public string? Direction { get; init; }

    /// <summary>true = chỉ chuyển nhượng mà cả hai đầu đều là CLB theo dõi (Chelsea ↔ Strasbourg).</summary>
    [FromQuery(Name = "internalOnly")]
    public bool? InternalOnly { get; init; }

    [FromQuery(Name = "season")]
    public string? Season { get; init; }

    [FromQuery(Name = "dateFrom")]
    public DateTime? DateFrom { get; init; }

    [FromQuery(Name = "dateTo")]
    public DateTime? DateTo { get; init; }
}