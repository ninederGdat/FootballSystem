namespace FotmobSync.Infrastructure.Resolvers.PositionRole;

public interface IPositionRoleResolver
{
    Task InitializeAsync();

    bool TryResolveRoleId(string? positionCode, out int roleId);
}