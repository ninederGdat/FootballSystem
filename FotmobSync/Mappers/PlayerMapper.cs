using FotmobSync.Models.Clean;
using FotmobSync.Models.Raw;
using FotmobSync.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FotmobSync.Mappers;

public static class PlayerMapper
{
    public static PlayerClean ToClean(this PlayerRaw? raw, long teamId)
    {
        if (raw == null)
        {
            return new PlayerClean { TeamId = teamId };
        }

        var clean = new PlayerClean
        {
            PlayerId = raw.Id,
            TeamId = teamId,
            Name = raw.Name?.Trim() ?? string.Empty,
            Status = raw.Status?.Trim() ?? "Healthy",
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