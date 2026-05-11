using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FotmobSync.Models.Clean
{
    [Table("positions")]
    public class PositionClean : BaseModel
    {
        [Column("position_code")]
        public string PositionCode { get; set; } = string.Empty;
        [Column("position_name")]
        public string PositionName { get; set; } = string.Empty;
        [Column("x_coord")]
        public double XCoord { get; set; } = 0.5;
        [Column("y_coord")]
        public double YCoord { get; set; } = 0.5;
        [Column("is_goalkeeper")]
        public bool IsGoalkeeper { get; set; } = false;
    }
}
