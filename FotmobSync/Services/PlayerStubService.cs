using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Services;
using Supabase.Postgrest;

public class PlayerStubService : IPlayerStubService
{
    private readonly Supabase.Client _supabase;
    public PlayerStubService(SupabaseClientFactory factory)
    {
        _supabase = factory.CreateServiceRoleClient();
    }

    public async Task<int> EnsureAsync(IEnumerable<PlayerStubInput> inputs, long teamId)
    {
        var distinct = inputs.GroupBy(x => x.PlayerId).Select(g => g.First()).ToList();
        if (distinct.Count == 0) return 0;

        var existing = new HashSet<long>();
        foreach (var chunk in distinct.Chunk(100))
        {
            var res = await _supabase.From<PlayerClean>()
                .Filter("player_id", Supabase.Postgrest.Constants.Operator.In,
                        chunk.Select(x => (object)x.PlayerId).ToList())
                .Get();
            foreach (var p in res.Models) existing.Add(p.PlayerId);
        }

        var now = DateTime.UtcNow;
        var stubs = distinct
            .Where(x => !existing.Contains(x.PlayerId))
            .Select(x => new PlayerClean
            {
                PlayerId = x.PlayerId,
                TeamId = teamId,
                Name = x.Name,
                Nationality = x.Nationality,
                ShirtNumber = x.ShirtNumber,
                MarketValue = x.MarketValue,
                Status = "unknown",          // kiểm tra CHECK constraint / giá trị hợp lệ
                TransferStatus = "none",     // dùng đúng giá trị mặc định PlayerSyncService đang dùng
                IsStub = true,
                CreatedAt = now,
                LastUpdated = now
            }).ToList();

        if (stubs.Count == 0) return 0;

        await _supabase.From<PlayerClean>().Upsert(stubs, new()
        {
            OnConflict = "player_id",
            DuplicateResolution = Supabase.Postgrest.QueryOptions.DuplicateResolutionType.IgnoreDuplicates
        });
        return stubs.Count;
    }



}