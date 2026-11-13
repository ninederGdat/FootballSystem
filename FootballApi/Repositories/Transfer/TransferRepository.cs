using FootballApi.DTOs.Players;
using FootballSystem.Shared.Infrastructure;
using System.Globalization;

namespace FootballApi.Repositories.Transfer;

public class TransferRepository : ITransferRepository
{
    private readonly Supabase.Client _client;

    public TransferRepository(SupabaseClientFactory factory) => _client = factory.CreateAnonClient();


    public async Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct = default)
    {
        var result = await _client.From<TransferClean>()
                                .Where(x => x.PlayerId == playerId)
                                .Get(ct);
        return result.Models.ToList();
    }

    public async Task<IReadOnlyList<TransferClean>> GetTransfersByDateRangeAsync(
        long teamId,
        DateTime from,
        DateTime toExclusive,
        bool? onLoan,
        CancellationToken ct = default)
    {
        var incomingQuery = _client.From<TransferClean>()
            .Filter("to_club_id", Supabase.Postgrest.Constants.Operator.Equals, teamId);
        var outgoingQuery = _client.From<TransferClean>()
            .Filter("from_club_id", Supabase.Postgrest.Constants.Operator.Equals, teamId);

        ApplyDateAndLoanFilters(incomingQuery, from, toExclusive, onLoan);
        ApplyDateAndLoanFilters(outgoingQuery, from, toExclusive, onLoan);

        var incomingTask = incomingQuery.Get(ct);
        var outgoingTask = outgoingQuery.Get(ct);
        await Task.WhenAll(incomingTask, outgoingTask);

        return incomingTask.Result.Models
            .Concat(outgoingTask.Result.Models)
            .GroupBy(x => x.Id)
            .Select(group => group.First())
            .OrderBy(x => x.TransferDate)
            .ThenBy(x => x.Id)
            .ToList();
    }

    private static void ApplyDateAndLoanFilters(
        Supabase.Postgrest.Interfaces.IPostgrestTable<TransferClean> query,
        DateTime from,
        DateTime toExclusive,
        bool? onLoan)
    {
        query.Filter(
            "transfer_date",
            Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual,
            from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        query.Filter(
            "transfer_date",
            Supabase.Postgrest.Constants.Operator.LessThan,
            toExclusive.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        if (onLoan is not null)
        {
            query.Filter(
                "on_loan",
                Supabase.Postgrest.Constants.Operator.Equals,
                onLoan.Value ? "true" : "false");
        }
    }

    public async Task<(IReadOnlyList<TransferClean> Items, int TotalCount)> SearchAsync(
     string playerName, long? playerId, long? fromClubId, long? toClubId,
     long? teamId, string? direction, IReadOnlyCollection<long>? internalClubIds,
     bool? onLoan, bool? contractExtension,
    string transferType, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken ct)
    {
        void ApplyFilters(Supabase.Postgrest.Interfaces.IPostgrestTable<TransferClean> q)
        {
            if (!string.IsNullOrWhiteSpace(playerName))
                q.Filter("player_name", Supabase.Postgrest.Constants.Operator.ILike, $"%{playerName}%");

            if (playerId is not null)
                q.Filter("player_id", Supabase.Postgrest.Constants.Operator.Equals, playerId.Value);

            if (fromClubId is not null)
                q.Filter("from_club_id", Supabase.Postgrest.Constants.Operator.Equals, fromClubId.Value);

            if (toClubId is not null)
                q.Filter("to_club_id", Supabase.Postgrest.Constants.Operator.Equals, toClubId.Value);

            if (teamId is not null)
            {
                // direction "in" = CLB nhận cầu thủ, "out" = CLB bán/cho mượn; không có = cả hai chiều.
                switch (direction)
                {
                    case "in":
                        q.Filter("to_club_id", Supabase.Postgrest.Constants.Operator.Equals, teamId.Value);
                        break;
                    case "out":
                        q.Filter("from_club_id", Supabase.Postgrest.Constants.Operator.Equals, teamId.Value);
                        break;
                    default:
                        q.Or(new List<Supabase.Postgrest.Interfaces.IPostgrestQueryFilter>
                        {
                            new Supabase.Postgrest.QueryFilter("from_club_id", Supabase.Postgrest.Constants.Operator.Equals, teamId.Value),
                            new Supabase.Postgrest.QueryFilter("to_club_id", Supabase.Postgrest.Constants.Operator.Equals, teamId.Value)
                        });
                        break;
                }
            }

            // Chuyển nhượng nội bộ: cả hai đầu đều thuộc tập CLB theo dõi.
            if (internalClubIds is { Count: > 0 })
            {
                var clubIds = internalClubIds.ToList();
                q.Filter("from_club_id", Supabase.Postgrest.Constants.Operator.In, clubIds);
                q.Filter("to_club_id", Supabase.Postgrest.Constants.Operator.In, clubIds);
            }
            if (onLoan is not null)
                q.Filter("on_loan", Supabase.Postgrest.Constants.Operator.Equals, onLoan.Value ? "true" : "false");

            if (contractExtension is not null)
                q.Filter("contract_extension", Supabase.Postgrest.Constants.Operator.Equals, contractExtension.Value ? "true" : "false");

            if (!string.IsNullOrWhiteSpace(transferType))
                q.Filter("transfer_type", Supabase.Postgrest.Constants.Operator.Equals, transferType);

            if (dateFrom is not null)
                q.Filter("transfer_date", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, dateFrom.Value.ToString("yyyy-MM-dd"));

            if (dateTo is not null)
                q.Filter("transfer_date", Supabase.Postgrest.Constants.Operator.LessThan, dateTo.Value.ToString("yyyy-MM-dd"));
        }

        var countQuery = _client.From<TransferClean>();
        ApplyFilters(countQuery);
        var totalCount = await countQuery.Count(Supabase.Postgrest.Constants.CountType.Exact, ct);

        var dataQuery = _client.From<TransferClean>();
        ApplyFilters(dataQuery);
        var result = await dataQuery
            .Order("transfer_date", Supabase.Postgrest.Constants.Ordering.Descending)
            .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
            .Range((page - 1) * pageSize, page * pageSize - 1)
            .Get(ct);

        return (result.Models, totalCount);
    }

    public async Task<Dictionary<long, TransferClean>> GetLatestByPlayerIdsAsync(
    IEnumerable<long> playerIds, CancellationToken ct = default)
    {
        var ids = playerIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<long, TransferClean>();

        var result = await _client.From<TransferClean>()
            .Filter("player_id", Supabase.Postgrest.Constants.Operator.In, ids)
            .Order("transfer_date", Supabase.Postgrest.Constants.Ordering.Descending)
            .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get(ct);

        // Mới nhất theo transfer_date, id là tie-breaker
        return result.Models
            .GroupBy(x => x.PlayerId)
            .ToDictionary(g => g.Key, g => g.First());
    }
}