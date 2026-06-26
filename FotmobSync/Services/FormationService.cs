using FotmobSync.Infrastructure;
using FotmobSync.Models.Clean;

namespace FotmobSync.Services;

public class FormationService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<FormationService> _logger;

    public FormationService(
        SupabaseClientFactory factory,
        ILogger<FormationService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    /// <summary>
    /// Resolve formation name
    /// → formation id
    ///
    /// Example:
    /// "4-2-3-1" → 1
    /// </summary>
    public async Task<long?> ResolveAsync(
        string formationName)
    {
        if (string.IsNullOrWhiteSpace(formationName))
        {
            return null;
        }

        try
        {
            var response = await _supabase
                .From<FormationClean>()
                .Where(x => x.Name == formationName)
                .Single();

            if (response is null)
            {
                _logger.LogWarning(
                    "Formation not found: {Formation}",
                    formationName);

                return null;
            }

            return response.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error resolving formation: {Formation}",
                formationName);

            return null;
        }
    }
}