using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FotmobSync.Models.Clean
{
    [Table("position_roles")]
    public class PositionRoleClean : BaseModel
    {
        [Column("position_code")]
        public string PositionCode { get; set; } = string.Empty;
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;
        [Column("role_short")]
        public string? RoleShort { get; set; }
        [Column("is_premium")]
        public bool IsPremium { get; set; } = false;
    }
}
