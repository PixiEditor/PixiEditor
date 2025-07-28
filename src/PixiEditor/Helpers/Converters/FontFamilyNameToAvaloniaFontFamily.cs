using System.Globalization;
using Avalonia.Media;
using Drawie.Backend.Core.Text;

namespace PixiEditor.Helpers.Converters;

internal class FontFamilyNameToAvaloniaFontFamily : SingleInstanceConverter<FontFamilyNameToAvaloniaFontFamily>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FontFamilyName familyName)
        {
            return new FontFamily(familyName.Name);
        }

        return value;
    }
}
