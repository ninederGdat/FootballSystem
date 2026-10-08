using FootballApi.DTOs.Players;
using FootballSystem.Shared.Players;

public class PlayerSummaryResponse
{
    public long PlayerId { get; set; }
    public string Name { get; set; } = default!;
    public PlayerTeamResponse? CurrentTeam { get; set; }
    public PlayerTeamResponse? ParentTeam { get; set; }
    public PlayerRelation Relation { get; set; }
    public DateOnly? LoanEndDate { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? Nationality { get; set; }
    public int? ShirtNumber { get; set; }
    public int? Age { get; set; }
    public string? Status { get; set; }
    public string? InjuryDescription { get; set; }   // đã chuẩn hóa ở server
    public decimal? MarketValue { get; set; }
    public DateOnly? ContractUntil { get; set; }
    public string? TransferStatus { get; set; }
}