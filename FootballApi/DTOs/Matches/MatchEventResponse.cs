namespace FootballApi.DTOs.Matches;

public record MatchEventResponse(
    long Id,
    long EventId,
    string Type,
    int Minute,
    int? StoppageTime,
    long? TeamId,
    MatchEventPlayerReferenceResponse? Player,
    MatchEventPlayerReferenceResponse? Assist,
    string? Description);