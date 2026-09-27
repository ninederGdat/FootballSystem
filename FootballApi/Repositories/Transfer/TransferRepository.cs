using FootballApi.DTOs.Players;
using FootballSystem.Shared.Infrastructure;

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

    public async Task<(IReadOnlyList<TransferClean> Items, int TotalCount)> SearchAsync(
     string playerName, long? playerId, long? fromClubId, long? toClubId, bool? onLoan, bool? contractExtension,
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
            .Order("id", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Range((page - 1) * pageSize, page * pageSize - 1)
            .Get(ct);

        return (result.Models, totalCount);
    }
}