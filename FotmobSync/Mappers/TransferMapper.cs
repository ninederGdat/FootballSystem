using FootballSystem.Shared.Models.Clean;
using FotmobSync.Models.Raw;
using Microsoft.Extensions.Logging;

namespace FotmobSync.Mappers;

/// <summary>
/// Mapper convert from TransferRaw (JSON from FotMob) to TransferClean (model for Database)
/// </summary>
public static class TransferMapper
{
    /// <summary>
    /// Convert TransferRaw to TransferClean
    /// </summary>
    public static TransferClean ToClean(this TransferRaw? raw)
    {
        if (raw == null)
        {
            return new TransferClean();
        }

        var (transferDate, transferDateIncomplete) = ParseFotmobDate(raw.TransferDate, raw.PlayerId, "transferDate");
        // TransferDate is Not Null in schema - if false cannot create suitable Record.
        if (transferDate == null)
        {
            return null;
        }


        var (periodStart, periodStartIncomplete) = ParseFotmobDate(raw.FromDate, raw.PlayerId, "fromDate");
        var (periodEnd, periodEndIncomplete) = ParseFotmobDate(raw.ToDate, raw.PlayerId, "toDate");

        var clean = new TransferClean
        {
            PlayerId = raw.PlayerId,
            PlayerName = raw.Name?.Trim() ?? string.Empty,
            TransferDate = transferDate.Value,
            HasIncompleteTimestamp = transferDateIncomplete || periodStartIncomplete || periodEndIncomplete,

            FromClubId = raw.FromClubId,
            FromClubName = raw.FromClubFullName?.Trim() is { Length: > 0 }
                            fromFullName ? fromFullName : raw.FromClub?.Trim() ?? string.Empty,
            ToClubId = raw.ToClubId,
            ToClubName = raw.ToClubFullName?.Trim() is { Length: > 0 }
                            toFullName ? toFullName : raw.ToClub?.Trim() ?? string.Empty,

            TransferType = raw.TransferType?.LocalizationKey?.Trim() ?? string.Empty, // "contract" | "on_loan"
            OnLoan = raw.OnLoan,

            ContractExtension = raw.ContractExtension,
            FeeValue = raw.Fee?.Value,
            FeeText = raw.Fee?.FeeText?.Trim() ?? raw.Fee?.LocalizedFeeText?.Trim(),

            MarketValue = raw.MarketValue,

            PeriodStart = periodStart,
            PeriodEnd = periodEnd,

            IsSystemGenerated = false, // Assuming this is a default value; adjust as needed

        };

        return clean;
    }


    /// <summary>
    /// Convert a list of TransferRaw to a list of TransferClean
    /// Which Record cannot parse transferDate will be skipped and not included in the result list.
    /// </summary>
    public static List<TransferClean> ToCleanList(this IEnumerable<TransferRaw> rawList)
    {
        if (rawList == null)
        {
            return new List<TransferClean>();
        }
        var cleanList = new List<TransferClean>();

        cleanList = rawList.Select(raw => raw.ToClean())
                    .Where(clean => clean != null) // Filter out nulls
                    .ToList()!; // Use ! to assert that the list is not null

        return cleanList;
    }

    /// <summary>
    /// Parse a Fotmob date string into a DateTime and determine if it has an incomplete timestamp.
    ///  </summary>
    public static (DateTime? transferDate, bool hasIncompleteTimestamp) ParseFotmobDate(string? fotmobDate, long playerId, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fotmobDate))
        {
            return (null, false);
        }

        //IF: Full ISO with hour (Example: "2026-06-15T09:22:32Z")
        if (DateTime.TryParse(
                fotmobDate,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                out var fullDateTime))
        {
            return (fullDateTime, false);
        }

        //IF: Date only (Example: "2026-06-15")
        if (DateTime.TryParse(
                fotmobDate,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var dateOnly))
        {
            return (dateOnly, false);
        }

        return (null, false);
    }


    /// <summary>
    /// Convert a list of TransferClean to a list of TransferUpsert
    /// > </summary>
    public static List<TransferUpsert> ToUpsertList(this IEnumerable<TransferClean> cleanList)
    {
        return cleanList?.Select(c => c.ToUpsert()).ToList() ?? new List<TransferUpsert>();
    }


    /// <summary>
    ///  Convert TransferClean to TransferUpsert
    /// </summary>
    /// <param name="clean"></param>
    /// <returns></returns>
    public static TransferUpsert ToUpsert(this TransferClean clean)
    {
        return new TransferUpsert
        {
            PlayerId = clean.PlayerId,
            PlayerName = clean.PlayerName,
            TransferDate = clean.TransferDate,
            HasIncompleteTimestamp = clean.HasIncompleteTimestamp,

            FromClubId = clean.FromClubId,
            FromClubName = clean.FromClubName,
            ToClubId = clean.ToClubId,
            ToClubName = clean.ToClubName,

            TransferType = clean.TransferType,
            OnLoan = clean.OnLoan,

            ContractExtension = clean.ContractExtension,
            FeeValue = clean.FeeValue,
            FeeText = clean.FeeText,

            MarketValue = clean.MarketValue,

            PeriodStart = clean.PeriodStart,
            PeriodEnd = clean.PeriodEnd,

            IsSystemGenerated = clean.IsSystemGenerated
        };
    }
}