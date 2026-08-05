using System.Text.Json;
using FootballSystem.Shared.Infrastructure;
using FotmobSync.Modules;
using FotmobSync.Services;
using Microsoft.Extensions.Logging;
using Supabase.Postgrest;
using FotmobSync.Mappers;
using FootballSystem.Shared.Models.Clean;
public class TeamSyncService : ITeamSyncService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<TeamSyncService> _logger;

    public TeamSyncService(
        SupabaseClientFactory factory,
        ILogger<TeamSyncService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task SyncAsync(
        TeamDataSnapshot snapshot)
    {
        var teamRaw = snapshot.TeamRaw;

        if (teamRaw == null)
            return;

        var teamClean = teamRaw.ToClean();

        var existingTeam =
            await LoadExistingTeamAsync(teamClean.TeamId);

        if (existingTeam != null &&
            IsTeamPayloadUnchanged(teamClean, existingTeam))
        {
            return;
        }

        await _supabase
            .From<TeamClean>()
            .Upsert(teamClean);
    }



    private async Task<TeamClean?> LoadExistingTeamAsync(long teamId)
    {
        var response = await _supabase
            .From<TeamClean>()
            .Filter("team_id", Constants.Operator.Equals, teamId)
            .Get();

        return response.Models?.FirstOrDefault();
    }

    private static bool IsTeamPayloadUnchanged(TeamClean incoming, TeamClean existing)
    {
        return incoming.TeamId == existing.TeamId
            && string.Equals(incoming.Name, existing.Name, StringComparison.Ordinal)
            && string.Equals(incoming.LogoUrl, existing.LogoUrl, StringComparison.Ordinal)
            && string.Equals(incoming.CoachName, existing.CoachName, StringComparison.Ordinal)
            && string.Equals(incoming.CoachNationality, existing.CoachNationality, StringComparison.Ordinal);
    }


}