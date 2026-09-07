namespace FootballApi.DTOs.Players;

public record PlayerProfileDTO
{
    public long PlayerId { get; init; }
    public string Name { get; init; }
    public int? ShirtNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Nationality { get; init; }
    public PlayerStatusDTO Status { get; init; }
    public PlayerTeamDTO? CurrentTeam { get; init; }
    public PlayerPositionDTO? PreferredPosition { get; init; }
    public PlayerContractDTO Contract { get; init; }
    public PlayerTransferStatusDTO? TransferStatus { get; init; }
}

public record PlayerStatusDTO
{
    public string Status { get; init; }          // "Active" / "Injured" / ...
    public string? InjuryDescription { get; init; }
}

public record PlayerTeamDTO
{
    public int TeamId { get; init; }
    public string TeamName { get; init; }
}

public record PlayerPositionDTO
{
    public string PositionCode { get; init; }
    public string PositionName { get; init; }
}

public record PlayerContractDTO
{
    public DateOnly? ContractUntil { get; init; }
    public decimal? MarketValue { get; init; }
}

public record PlayerTransferStatusDTO
{
    public string CurrentTeam { get; init; }
    public string Status { get; init; }
    public DateOnly PeriodEnd { get; init; }
}