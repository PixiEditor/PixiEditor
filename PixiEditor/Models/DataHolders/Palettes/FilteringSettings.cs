using PixiEditor.Models.Enums;
using System;
using System.Linq;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class FilteringSettings
    {
        public ColorsNumberMode ColorsNumberMode { get; set; }
        public int ColorsCount { get; set; }
        public string[] Tags { get; set; }

        public string Name { get; set; }

        public FilteringSettings(ColorsNumberMode colorsNumberMode, int colorsCount, string[] tags, string name)
        {
            ColorsNumberMode = colorsNumberMode;
            ColorsCount = colorsCount;
            Tags = tags;
            Name = name;
        }

        public bool Filter(Palette palette)
        {
            // Lexical comparison
            bool result = palette.Title.Contains(Name, StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(Name)) result = true;

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

            if (Tags.Length > 0)
            {
                result = Tags.All(tag => palette.Tags.Contains(tag));
            }

            return result;
        }
    }
}
