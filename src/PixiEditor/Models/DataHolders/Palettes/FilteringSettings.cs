using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DataHolders.Palettes;

internal class FilteringSettings
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
        bool result = string.IsNullOrWhiteSpace(Name) || palette.Name.Contains(Name, StringComparison.OrdinalIgnoreCase);

        if (!result)
        {
            return false;
        }

        result = (ShowOnlyFavourites && palette.IsFavourite) || !ShowOnlyFavourites;

        if (!result)
        {
            return false;
        }

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
