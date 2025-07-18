namespace PixiEditor.Extensions.CommonApi.Palettes;

public partial class FilteringSettings
{
    public FilteringSettings()
    {

    }
    
    public FilteringSettings(ColorsNumberMode colorsNumberMode, int colorsCount, string name, bool showOnlyFavourites, List<string> favourites)
    {
        ColorsNumberMode = colorsNumberMode;
        ColorsCount = colorsCount;
        Name = name;
        ShowOnlyFavourites = showOnlyFavourites;
        Favourites = favourites;
    }

    public bool Filter(IPalette palette)
    {
        // Lexical comparison
        bool result = string.IsNullOrWhiteSpace(Name) ||
                      palette.Name.Contains(Name, StringComparison.OrdinalIgnoreCase);

        if (!result)
        {
            return false;
        }

        result = (ShowOnlyFavourites && Favourites != null && Favourites.Contains(palette.Name)) || !ShowOnlyFavourites;

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
