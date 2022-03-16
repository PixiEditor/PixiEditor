using PixiEditor.Models.Enums;
using System;
using System.Linq;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class FilteringSettings
    {
        public ColorsNumberMode ColorsNumberMode { get; set; }
        public int ColorsCount { get; set; }
        public string Name { get; set; }

        public bool ShowOnlyFavourites { get; set; }

        public FilteringSettings(ColorsNumberMode colorsNumberMode, int colorsCount, string name, bool showOnlyFavourites)
        {
            ColorsNumberMode = colorsNumberMode;
            ColorsCount = colorsCount;
            Name = name;
            ShowOnlyFavourites = showOnlyFavourites;
        }

        public bool Filter(Palette palette)
        {
            // Lexical comparison
            bool result = string.IsNullOrWhiteSpace(Name) || palette.Title.Contains(Name, StringComparison.OrdinalIgnoreCase);

            result = (ShowOnlyFavourites && palette.IsFavourite) || !ShowOnlyFavourites;

            switch (ColorsNumberMode)
            {
                case ColorsNumberMode.Any:
                    break;
                case ColorsNumberMode.Max:
                    result = palette.Colors.Count <= ColorsCount;
                    break;
                case ColorsNumberMode.Min:
                    result = palette.Colors.Count >= ColorsCount;
                    break;
                case ColorsNumberMode.Exact:
                    result = palette.Colors.Count == ColorsCount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}
