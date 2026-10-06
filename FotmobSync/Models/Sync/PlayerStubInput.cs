public sealed record PlayerStubInput(
    long PlayerId, string Name, string? Nationality,
    int? ShirtNumber, decimal? MarketValue);