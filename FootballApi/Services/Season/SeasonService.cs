using FootballApi.Common.Exceptions;
using FootballApi.Configuration;
using Microsoft.Extensions.Options;

namespace FootballApi.Services.Season;

public interface ISeasonService
{
    SeasonDefinition Resolve(string seasonCode);
    SeasonDefinition ResolveCurrent(DateTime utcNow);
    IReadOnlyList<SeasonDefinition> GetAvailableSeasons();
}

public readonly record struct SeasonDateRange(DateTime? FromDate, DateTime? ToDateExclusive)
{
    public static SeasonDateRange Constrain(
        SeasonDefinition season,
        DateTime? fromDate = null,
        DateTime? toDateExclusive = null)
    {
        var constrainedFrom = fromDate is null || fromDate.Value < season.StartDate
            ? season.StartDate
            : fromDate.Value;
        var constrainedTo = toDateExclusive is null || toDateExclusive.Value > season.EndDate
            ? season.EndDate
            : toDateExclusive.Value;

        return new SeasonDateRange(constrainedFrom, constrainedTo);
    }
}

public sealed class SeasonResolver
{
    private readonly SeasonOptions _options;

    public SeasonResolver(SeasonOptions options)
    {
        _options = options;
    }

    public SeasonDefinition Resolve(string seasonCode)
    {
        if (string.IsNullOrWhiteSpace(seasonCode))
        {
            throw new BadRequestException("Season code is required.");
        }

        var normalized = seasonCode.Trim();
        var season = _options.Seasons.FirstOrDefault(x =>
            string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase));

        if (season is null)
        {
            throw new BadRequestException($"Season '{seasonCode}' was not found.");
        }

        return season;
    }

    public SeasonDefinition ResolveCurrent(DateTime utcNow)
    {
        var season = _options.Seasons.FirstOrDefault(x =>
            x.StartDate <= utcNow && utcNow < x.EndDate);

        return season ?? throw new BadRequestException(
            "No configured season contains the current UTC date.");
    }

    public IReadOnlyList<SeasonDefinition> GetAvailableSeasons()
    {
        return _options.Seasons
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.EndDate)
            .ToList();
    }
}

public sealed class SeasonService : ISeasonService
{
    private readonly SeasonResolver _resolver;

    public SeasonService(IOptions<SeasonOptions> options)
    {
        _resolver = new SeasonResolver(options.Value);
    }

    public IReadOnlyList<SeasonDefinition> GetAvailableSeasons()
    {
        return _resolver.GetAvailableSeasons();
    }

    public SeasonDefinition Resolve(string seasonCode)
    {
        return _resolver.Resolve(seasonCode);
    }

    public SeasonDefinition ResolveCurrent(DateTime utcNow)
    {
        return _resolver.ResolveCurrent(utcNow);
    }
}
