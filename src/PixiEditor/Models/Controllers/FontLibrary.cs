using Avalonia.Media;
using Drawie.Backend.Core.Text;
using FontStyle = Drawie.Backend.Core.Text.FontStyle;

namespace PixiEditor.Models.Controllers;

public static class FontLibrary
{
    private static List<FontFamilyName> _customFonts = new List<FontFamilyName>();
    private static List<FontFamilyName> _allFonts = new List<FontFamilyName>();

    public static FontFamilyName DefaultFontFamily { get; } = new FontFamilyName("Arial");

    public static FontFamilyName[] SystemFonts { get; } = FontManager.Current.SystemFonts.Select(x => new FontFamilyName(x.Name)).ToArray();
    
    public static IReadOnlyList<FontFamilyName> CustomFonts => _customFonts;

    public static FontFamilyName[] AllFonts
    {
        get
        {
            if (_allFonts.Count != SystemFonts.Length + CustomFonts.Count)
            {
                _allFonts = SystemFonts.Concat(CustomFonts).ToList();
            }

            return _allFonts.ToArray();
        }
    }

    public static event Action<FontFamilyName> FontAdded;

    public static bool TryAddCustomFont(FontFamilyName fontFamily)
    {
        if (!CustomFonts.Any(x => x.Name == fontFamily.Name && x.FontUri == fontFamily.FontUri))
        {
            _customFonts.Add(fontFamily);
            FontAdded?.Invoke(fontFamily);
            return true;
        }
        
        return false;
    }

    public static FontStyle[] GetAvailableFontStyles(string fontFamily)
    {
        return Font.GetAvailableFontStyles(fontFamily);
    }

    public static FontStyle GetClosestMatchingFontStyle(string familyName, FontStyleWeight fontWeight, FontStyleSlant fontSlant, FontStyleWidth fontWidth)
    {
        var styles = Font.GetAvailableFontStyles(familyName);

        FontStyle closestMatchingStyle = styles.FirstOrDefault();
        int closestDistance = int.MaxValue;

        foreach (var style in styles)
        {
            int distance = Math.Abs((int)style.Weight - (int)fontWeight) +
                           Math.Abs((int)style.Slant - (int)fontSlant) +
                           Math.Abs((int)style.Width - (int)fontWidth);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestMatchingStyle = style;
            }
        }
        return closestMatchingStyle;
    }
}
