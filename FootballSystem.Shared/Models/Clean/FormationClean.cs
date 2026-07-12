using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballSystem.Shared.Models.Clean;

[Table("formations")]
public class FormationClean : BaseModel
{
    [Column("id")]
    public long Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    [Column("is_popular")]
    public bool IsPopular { get; set; } = false;
}