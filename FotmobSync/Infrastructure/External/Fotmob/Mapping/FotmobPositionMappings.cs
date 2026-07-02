namespace FotmobSync.Infrastructure.External.Fotmob.Mapping;

internal static class FotmobPositionMappings
{
    internal static readonly IReadOnlyDictionary<int, string> PositionCodes =
        new Dictionary<int, string>
        {
            // Goalkeeper
            [11] = "GK",

            // Back Four
            [32] = "RB",
            [34] = "CB",
            [36] = "CB",
            [38] = "LB",

            // Back Three
            [33] = "CB",
            [35] = "CB",
            [37] = "CB",

            // Wing Back
            [62] = "RWB",
            [68] = "LWB",

            // Defensive Midfield
            [64] = "DM",
            [66] = "DM",

            // Midfield
            [72] = "RM",
            [74] = "CM",
            [76] = "CM",
            [78] = "LM",

            // Attacking Midfield
            [83] = "RW",
            [84] = "AM",
            [85] = "AM",
            [86] = "AM",
            [87] = "LW",

            // Striker
            [105] = "ST",
            [115] = "ST"
        };
}
