using FootballApi.DTOs.Players;
using FootballSystem.Shared.Infrastructure;

namespace FootballApi.Repositories.Transfer;

public class TransferRepository : ITransferRepository
{
    private readonly Supabase.Client _client;

    public TransferRepository(SupabaseClientFactory factory) => _client = factory.CreateServiceRoleClient();


    public async Task<List<TransferClean>> GetTransfersByPlayerIdAsync(long playerId, CancellationToken ct = default)
    {
        var result = await _client.From<TransferClean>()
                                .Where(x => x.PlayerId == playerId)
                                .Get(ct);
        return result.Models.ToList();
    }


}