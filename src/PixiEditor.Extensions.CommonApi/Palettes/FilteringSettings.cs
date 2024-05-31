using ProtoBuf;

namespace PixiEditor.Extensions.CommonApi.Palettes;

[ProtoContract]
public sealed class FilteringSettings
{
    [ProtoMember(1)]
    public ColorsNumberMode ColorsNumberMode { get; set; }
    
    [ProtoMember(2)]
    public int ColorsCount { get; set; }
    
    [ProtoMember(3)]
    public string Name { get; set; }
    
    [ProtoMember(4)]
    public bool ShowOnlyFavourites { get; set; }
    
    [ProtoMember(5)]
    public List<string> Favourites { get; set; }
    
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
        bool result = string.IsNullOrWhiteSpace(Name) || palette.Name.Contains(Name, StringComparison.OrdinalIgnoreCase);

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
