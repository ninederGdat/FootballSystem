using Microsoft.Extensions.Options;

namespace FootballApi.Configuration;

public sealed record SeasonDefinition(
    string Code,
    string Name,
    DateTime StartDate,
    DateTime EndDate);

public sealed class SeasonOptions
{
    public const string SectionName = "Seasons";

    public List<SeasonDefinition> Seasons { get; set; } = [];
}

public sealed class SeasonOptionsValidator : IValidateOptions<SeasonOptions>
{
    public ValidateOptionsResult Validate(string? name, SeasonOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("Season configuration is required.");
        }

        var failures = new List<string>();

        if (options.Seasons is null || options.Seasons.Count == 0)
        {
            failures.Add("At least one season definition must be configured.");
            return ValidateOptionsResult.Fail(failures);
        }

        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = options.Seasons
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.EndDate)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            var season = ordered[i];

            if (string.IsNullOrWhiteSpace(season.Code))
            {
                failures.Add($"Season at index {i} is missing a code.");
            }

            if (string.IsNullOrWhiteSpace(season.Name))
            {
                failures.Add($"Season '{season.Code}' is missing a name.");
            }

            if (season.StartDate >= season.EndDate)
            {
                failures.Add($"Season '{season.Code}' must have StartDate earlier than EndDate.");
            }

            if (!seenCodes.Add(season.Code.Trim()))
            {
                failures.Add($"Season code '{season.Code}' is duplicated.");
            }

            if (i > 0)
            {
                var previous = ordered[i - 1];
                var overlap = previous.EndDate > season.StartDate;
                if (overlap)
                {
                    failures.Add($"Season ranges overlap: '{previous.Code}' and '{season.Code}'.");
                }
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
