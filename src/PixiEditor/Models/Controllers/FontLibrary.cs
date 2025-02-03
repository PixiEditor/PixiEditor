using System.Collections.ObjectModel;
using Avalonia.Media;
using Drawie.Backend.Core.Text;

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
}
