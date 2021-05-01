namespace Fitmeplan.Common
{
    public static class ColorServiceExtension
    {
        public static string[] Colors { get; }
        public static string Disabled { get; }
        public static string Unassigned { get; }

        static ColorServiceExtension()
        {
            Colors = new[]
            {
                "#b1dafa",
                "#726bb1",
                "#7fafb1",
                "#d9c3ff",
                "#a9b6fe",
                "#a2acb1",
                "#a97f67",
                "#a6dad5",
                "#7ff2ba",
                "#a489c5",
                "#cece91",
                "#ffeaa7",
                "#c6b6b1",
                "#c5e1a4",
                "#d689ab",
                "#bdd9a0",
                "#d7d995",
                "#be9d68",
                "#81cdf2",
                "#beabe0",
                "#c6b6b1",
                "#f29c9a",
                "#a1cfa3",
                "#9fa8da",
                "#cd93d7"
            };
            Disabled = "#A9A9A9";
            Unassigned = "#ff3232";
        }

        public static string GetColor(this int catalogItemId)
        {
            var index = catalogItemId % Colors.Length;
            return Colors[index];
        }
    }
}
