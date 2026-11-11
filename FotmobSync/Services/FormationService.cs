using System.Text.RegularExpressions;
using FootballSystem.Shared.Infrastructure;
using FootballSystem.Shared.Models.Clean;

namespace FotmobSync.Services;

public class FormationService
{
    // Dạng Fotmob trả: "4-3-3", "4-2-3-1", "4-3-2-1"... (không tính thủ môn).
    private static readonly Regex FormationPattern =
        new(@"^\d(-\d){1,5}\z", RegexOptions.Compiled);

    private readonly Supabase.Client _supabase;
    private readonly ILogger<FormationService> _logger;

    public FormationService(
        SupabaseClientFactory factory,
        ILogger<FormationService> logger)
    {
        _supabase = factory.CreateServiceRoleClient();
        _logger = logger;
    }

    public static bool IsValidFormationName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && FormationPattern.IsMatch(name);

    /// <summary>
    /// Resolve formation name → formation id. Ví dụ: "4-2-3-1" → 1.
    /// Formation chưa có trong DB và đúng dạng "d-d-d" thì tự tạo (is_popular = false).
    /// </summary>
    public async Task<long?> ResolveAsync(string formationName)
    {
        if (string.IsNullOrWhiteSpace(formationName))
            return null;

        var name = formationName.Trim();

        try
        {
            var existingId = await FindIdAsync(name);
            if (existingId is not null)
                return existingId;

            if (!IsValidFormationName(name))
            {
                _logger.LogWarning(
                    "Formation '{Formation}' is not in the database and is not a valid formation name; not created.",
                    name);
                return null;
            }

            await CreateAsync(name);
            return await FindIdAsync(name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving formation: {Formation}", name);
            return null;
        }
    }

    private async Task<long?> FindIdAsync(string name)
    {
        var response = await _supabase
            .From<FormationClean>()
            .Where(x => x.Name == name)
            .Get();

        return response.Models.FirstOrDefault()?.Id;
    }

    private async Task CreateAsync(string name)
    {
        try
        {
            await _supabase
                .From<FormationUpsert>()
                .Insert(new FormationUpsert { Name = name, IsPopular = false });

            _logger.LogInformation("Formation '{Formation}' created automatically.", name);
        }
        catch (Exception ex)
        {
            // Có thể một tiến trình khác vừa tạo (name là UNIQUE): caller đọc lại để biết.
            _logger.LogWarning(ex, "Could not insert formation '{Formation}'; re-reading.", name);
        }
    }
}