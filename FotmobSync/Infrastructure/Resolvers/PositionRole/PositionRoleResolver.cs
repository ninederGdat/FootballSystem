using FotmobSync.Models.Clean;
using FootballSystem.Shared.Infrastructure;

namespace FotmobSync.Infrastructure.Resolvers.PositionRole;

public sealed class PositionRoleResolver : IPositionRoleResolver
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<PositionRoleResolver> _logger;

    private Dictionary<string, int> _defaultRoles = new();

    public PositionRoleResolver(
       SupabaseClientFactory factory,
        ILogger<PositionRoleResolver> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var response = await _supabase
            .From<PositionRoleClean>()
            .Where(x => x.IsDefault == true)
            .Get();

        _defaultRoles = response.Models.ToDictionary(
            x => x.PositionCode,
            x => x.Id);

        _logger.LogInformation(
            "Loaded {Count} default position roles.",
            _defaultRoles.Count);
    }

    public bool TryResolveRoleId(string? positionCode, out int roleId)
    {
        roleId = default;

        if (string.IsNullOrWhiteSpace(positionCode))
            return false;

        return _defaultRoles.TryGetValue(positionCode, out roleId);
    }
}