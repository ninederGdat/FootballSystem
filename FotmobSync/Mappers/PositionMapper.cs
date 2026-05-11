using FotmobSync.Models.Raw;
using System;
using System.Collections.Generic;
using System.Text;

namespace FotmobSync.Mappers
{
    // Mappers/PositionMapper.cs
    public static class PositionMapper
    {
        public static string? ToPositionCode(string? fotmobKey)
        {
            if (string.IsNullOrWhiteSpace(fotmobKey))
                return null;

            var key = fotmobKey.ToLower().Trim();

            return key switch
            {
                "goalkeeper" or "keeper" or "keeper_long" => "GK",
                "rightback" or "right back" => "RB",
                "leftback" or "left back" => "LB",
                "centerback" or "centreback" or "defender" => "CB",
                "rightwinger" or "right wing" => "RW",
                "leftwinger" or "left wing" => "LW",
                "centermidfielder" or "central midfielder" or "midfielder" => "CM",
                "centerdefensivemidfielder" or "defensivemidfielder" or "dm" => "DM",
                "centerattackingmidfielder" or "attackingmidfielder" or "am" => "AM",
                "striker" or "forward" or "attacker" => "ST",
                "coach" or "manager" or "head coach" or "headcoach" => "COACH",
                _ => key.ToUpper()   // Fallback quan trọng
            };
        }

        public static string? ToPositionName(string? fotmobLabel)
        {
            return fotmobLabel?.Trim() ?? "Unknown Position";
        }

        public static string? GetPrimaryPositionCode(PositionDescriptionRaw? desc)
        {
            return ToPositionCode(desc?.PrimaryPosition?.Key);
        }
    }
}
