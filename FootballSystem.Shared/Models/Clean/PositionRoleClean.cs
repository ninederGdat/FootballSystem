using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballSystem.Shared.Models.Clean
{
    [Table("position_roles")]
    public class PositionRoleClean : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public int Id { get; set; }
        [Column("position_code")]
        public string PositionCode { get; set; } = string.Empty;
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;
        [Column("role_short")]
        public string? RoleShort { get; set; }
        [Column("is_premium")]
        public bool IsPremium { get; set; } = false;
        [Column("is_default")]
        public bool IsDefault { get; set; } = false;
    }
}
