namespace FootballApi.DTOs.Responses;

using FootballApi.DTOs.Players;

// Wrapper cho single-resource response, đồng bộ với LineupResponse/MatchResponse
public record PlayerProfileResponse
{
    public PlayerProfileDTO Player { get; init; } = default!;
}

public record PlayerAppearancesResponse
{
    public int PlayerId { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyList<PlayerAppearanceDTO> Appearances { get; init; } = [];
}

public record PlayerSearchResponse
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyList<PlayerSummaryDTO> Items { get; init; } = [];
}