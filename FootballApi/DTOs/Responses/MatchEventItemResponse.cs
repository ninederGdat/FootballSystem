namespace FootballApi.DTOs.Responses;

public record PlayerRef(long? Id, string? Name);

public record MatchEventItemResponse(
    long Id,
    long EventId,
    string Type,
    int Minute,
    int? StoppageTime,
    long? TeamId,
    PlayerRef? Player,
    PlayerRef? Assist,
    string? Description);