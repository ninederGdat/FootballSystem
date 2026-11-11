using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

/// <summary>
/// Model ghi cho <c>public.formations</c>. Không có id vì cột id là GENERATED ALWAYS.
/// </summary>
[Table("formations")]
public class FormationUpsert : BaseModel
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("is_popular")]
    public bool IsPopular { get; set; }
}