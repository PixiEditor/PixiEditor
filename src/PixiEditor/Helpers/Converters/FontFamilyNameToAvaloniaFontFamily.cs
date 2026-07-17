using System.Globalization;
using Avalonia.Media;
using Drawie.Backend.Core.Text;

namespace PixiEditor.Helpers.Converters;

internal class FontFamilyNameToAvaloniaFontFamily : SingleInstanceConverter<FontFamilyNameToAvaloniaFontFamily>
{
    private static HashSet<FontFamilyName> cachedValidFonts = new HashSet<FontFamilyName>();
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FontFamilyName familyName)
        {
            if(cachedValidFonts.Contains(familyName))
            {
                return new FontFamily(familyName.Name);
            }

            if(!IsValidFontFamily(familyName))
            {
                return FontFamily.Default;
            }

            return new FontFamily(familyName.Name);
        }

        return value;
    }

    private static bool IsValidFontFamily(FontFamilyName familyName)
    {
        try
        {
            using var font = Font.FromFontFamily(familyName);
            if(font == null)
            {
                return false;
            }

            cachedValidFonts.Add(familyName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
