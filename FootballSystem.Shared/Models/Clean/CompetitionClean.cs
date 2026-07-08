using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FotmobSync.Models.Clean;

[Table("competitions")]
public class CompetitionClean : BaseModel
{
    [Column("competition_id")]
    public long CompetitionId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; }
}
