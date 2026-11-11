using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;
using FotmobSync.Mappers;
using FotmobSync.Modules;
using Supabase.Postgrest;

namespace FotmobSync.Services;

/// <summary>
/// Ghi điểm luân lưu vào matches.penalty_home / penalty_away.
/// Chỉ update đúng hai cột này (không đi qua model upsert của fixtures) để fixtures sync không ghi đè.
/// </summary>
public class MatchPenaltyScoreSyncService : IMatchPenaltyScoreSyncService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<MatchPenaltyScoreSyncService> _logger;

    public MatchPenaltyScoreSyncService(
        SupabaseClientFactory factory,
        ILogger<MatchPenaltyScoreSyncService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task SyncAsync(MatchDetailSnapshot snapshot, long matchId)
    {
        var score = snapshot.MatchEventsRaw.TryGetScore();
        if (score is null)
            return;

        try
        {
            await _supabase.From<MatchClean>()
                .Filter("match_id", Constants.Operator.Equals, matchId)
                .Set(x => x.PenaltyHome, score.Value.Home)
                .Set(x => x.PenaltyAway, score.Value.Away)
                .Update();

            _logger.LogInformation(
                "Match {MatchId}: penalty shootout {Home}-{Away} saved.",
                matchId, score.Value.Home, score.Value.Away);
        }
        catch (Exception ex)
        {
            // Lỗi ghi luân lưu không được chặn việc đánh dấu lineup hoàn tất.
            _logger.LogError(ex, "Match {MatchId}: cannot save penalty shootout score.", matchId);
        }
    }
}