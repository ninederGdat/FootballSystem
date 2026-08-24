using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("transfers")]
public class TransferClean : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")]
    public long Id { get; set; }

    [Column("player_id")]
    public long PlayerId { get; set; }
    [Column("player_name")]
    public string PlayerName { get; set; } = string.Empty;

    [Column("transfer_date")]
    public DateTime TransferDate { get; set; }
    [Column("has_incomplete_timestamp")]
    public bool HasIncompleteTimestamp { get; set; }

    [Column("from_club_id")]
    public long FromClubId { get; set; }
    [Column("from_club_name")]
    public string FromClubName { get; set; } = string.Empty;
    [Column("to_club_id")]
    public long ToClubId { get; set; }
    [Column("to_club_name")]
    public string ToClubName { get; set; } = string.Empty;
    [Column("transfer_type")]
    public string TransferType { get; set; } = string.Empty; // "contract" | "on_loan"
    [Column("on_loan")]
    public bool OnLoan { get; set; }

    [Column("contract_extension")]
    public bool? ContractExtension { get; set; }

    [Column("fee_value")]
    public decimal? FeeValue { get; set; }
    [Column("fee_text")]
    public string? FeeText { get; set; }

    [Column("market_value")]
    public decimal? MarketValue { get; set; }
    [Column("period_start")]
    public DateTime? PeriodStart { get; set; } // fromDate
    [Column("period_end")]
    public DateTime? PeriodEnd { get; set; }    // toDate

    [Column("is_system_generated")]
    public bool IsSystemGenerated { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("last_updated")]
    public DateTime LastUpdated { get; set; }
}