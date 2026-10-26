using FotmobSync.Models.Raw;
using FotmobSync.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FootballSystem.Shared.Models.Clean;

namespace FotmobSync.Mappers;

public static class PlayerMapper
{
    public static PlayerClean ToClean(this PlayerRaw? raw, long teamId)
    {
        if (raw == null)
        {
            return new PlayerClean { TeamId = teamId };
        }

        var (status, injuryDescription) = MapPlayerStatus(raw);

        var clean = new PlayerClean
        {
            PlayerId = raw.Id,
            TeamId = teamId,
            Name = raw.Name?.Trim() ?? string.Empty,
            Status = status,
            InjuryDescription = injuryDescription,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,

            PreferredPositionCode = PositionMapper.ToPositionCode(
                raw.PositionDescription?.PrimaryPosition?.Key),

            // Market Value - Lấy giá trị mới nhất
            MarketValue = GetLatestMarketValue(raw.MarketValues),

            // Nationality - Ưu tiên từ meta.personJSONLD
            Nationality = GetNationality(raw)
        };

        // DateOfBirth
        if (raw.BirthDate?.UtcTime != null &&
            DateOnly.TryParse(raw.BirthDate.UtcTime.AsSpan(0, 10), out var dob))
        {
            clean.DateOfBirth = dob;
        }

        // Contract Until
        if (raw.ContractEnd?.UtcTime != null &&
            DateOnly.TryParse(raw.ContractEnd.UtcTime.AsSpan(0, 10), out var contractEnd))
        {
            clean.ContractUntil = contractEnd;
        }

        // Shirt Number
        var shirtInfo = raw.PlayerInformation?.FirstOrDefault(p =>
            p.Title?.Contains("shirt", StringComparison.OrdinalIgnoreCase) == true);

        if (shirtInfo?.Value?.NumberValue != null)
        {
            clean.ShirtNumber = shirtInfo.Value.NumberValue;
        }

        return clean;
    }

    /// <summary>
    /// Fotmob: <see cref="PlayerRaw.InjuryInformation"/> null → Healthy; otherwise Injured.
    /// Description is "{name} (Expected return: {value})", where value is a date or Fotmob's
    /// fallback text (e.g. "Doubtful"). The "(Expected return: ...)" part is omitted when empty.
    /// </summary>
    private static (string Status, string? InjuryDescription) MapPlayerStatus(PlayerRaw raw)
    {
        if (raw.InjuryInformation == null)
            return ("Healthy", null);

        var name = raw.InjuryInformation.Name?.Trim() ?? string.Empty;
        var expectedReturn = FormatExpectedReturn(raw.InjuryInformation.ExpectedReturn);

        return ("Injured", BuildInjuryDescription(name, expectedReturn));
    }

    private static string? BuildInjuryDescription(string name, string expectedReturn)
    {
        var hasName = name.Length > 0;
        var hasReturn = expectedReturn.Length > 0;

        return (hasName, hasReturn) switch
        {
            (true, true) => $"{name} (Expected return: {expectedReturn})",
            (true, false) => name,
            (false, true) => $"Expected return: {expectedReturn}",
            _ => null
        };
    }

    private static string FormatExpectedReturn(JsonElement? expectedReturn)
    {
        if (!expectedReturn.HasValue ||
            expectedReturn.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return string.Empty;

        var value = expectedReturn.Value;

        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return NormalizeReturnDateText(value.GetString());

            case JsonValueKind.Object:
                // A concrete date wins over the fallback text when both are present
                if (TryGetNonEmptyString(value, "utcTime", out var utc))
                    return NormalizeReturnDateText(utc);

                if (TryGetNonEmptyString(value, "expectedReturnFallback", out var fallback))
                    return fallback.Trim();

                return string.Empty;

            default:
                return string.Empty;
        }
    }

    private static bool TryGetNonEmptyString(JsonElement obj, string property, out string value)
    {
        if (obj.TryGetProperty(property, out var element) &&
            element.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(element.GetString()))
        {
            value = element.GetString()!;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string NormalizeReturnDateText(string? utcTime)
    {
        if (string.IsNullOrWhiteSpace(utcTime))
            return string.Empty;

        var span = utcTime.AsSpan();
        if (span.Length >= 10 && DateOnly.TryParse(span[..10], out var d))
            return d.ToString("yyyy-MM-dd");

        return utcTime.Trim();
    }

    private static decimal? GetLatestMarketValue(MarketValuesRaw? marketValues)
    {
        if (marketValues?.Values == null || marketValues.Values.Count == 0)
            return null;

        // Lấy item có date mới nhất
        var latest = marketValues.Values
            .Where(v => v.Value.HasValue)
            .OrderByDescending(v => v.Date)
            .FirstOrDefault();

        return latest?.Value;
    }

    private static string? GetNationality(PlayerRaw raw)
    {
        // Ưu tiên từ meta.personJSONLD
        if (!string.IsNullOrEmpty(raw.Meta?.PersonJSONLD?.Nationality?.Name))
            return raw.Meta.PersonJSONLD.Nationality.Name.Trim();

        // Fallback từ playerInformation hoặc ccode
        var nationalityField = raw.PlayerInformation?.FirstOrDefault(info =>
            info.Title?.Contains("nationality", StringComparison.OrdinalIgnoreCase) == true ||
            info.Title?.Contains("country", StringComparison.OrdinalIgnoreCase) == true);

        if (nationalityField?.Value != null)
        {
            return nationalityField.Value.NumberValue?.ToString()
                ?? nationalityField.Value.ToString();
        }

        return raw.CountryCode?.Trim();
    }

    public static List<PlayerClean> ToCleanList(this IEnumerable<PlayerRaw> rawList, long teamId)
    {
        return rawList.Select(raw => raw.ToClean(teamId)).ToList();
    }
}