using FootballSystem.Shared.Players;

namespace FootballApi.DTOs.Players;

public record PlayerProfileResponse
{
    public long PlayerId { get; init; }
    public string Name { get; init; }
    public int? ShirtNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Nationality { get; init; }
    public PlayerStatusResponse Status { get; init; }
    public PlayerTeamResponse? CurrentTeam { get; init; }
    public PlayerTeamResponse? ParentTeam { get; init; }   // CLB chủ quản khi đang mượn
    public PlayerRelation Relation { get; init; }
    public DateOnly? LoanEndDate { get; init; }
    public PlayerPositionResponse? PreferredPosition { get; init; }
    public PlayerContractResponse Contract { get; init; }
    public PlayerTransferStatusResponse? TransferStatus { get; init; }
}