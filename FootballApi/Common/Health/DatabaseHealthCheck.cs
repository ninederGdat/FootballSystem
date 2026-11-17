using FootballApi.Repositories.Team;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FootballApi.Common.Health;

/// <summary>
/// Readiness check: can the API reach Supabase? Runs a cheap filtered query that
/// matches no row (team_id = 0), so it only proves connectivity + valid key.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    private readonly ITeamRepository _teams;
    private readonly ILogger<DatabaseHealthCheck> _logger;
    private readonly TimeSpan _timeout;

    public DatabaseHealthCheck(
        ITeamRepository teams,
        ILogger<DatabaseHealthCheck> logger,
        TimeSpan? timeout = null)
    {
        _teams = teams;
        _logger = logger;
        _timeout = timeout ?? DefaultTimeout;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _teams.GetByIdAsync(0, cancellationToken)
                .WaitAsync(_timeout, cancellationToken);
            return HealthCheckResult.Healthy("Database reachable.");
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Database health check timed out after {Timeout}", _timeout);
            return HealthCheckResult.Unhealthy("Database check timed out.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // caller aborted the request, not a DB problem
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database unreachable.");
        }
    }
}