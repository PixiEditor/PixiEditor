using System.Globalization;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Palettes;

namespace PixiEditor.Helpers.Converters;

internal class PaletteColorEqualsConverter : SingleInstanceMultiValueConverter<PaletteColorEqualsConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new[] { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return false;
        }

        PaletteColor color = values[0] as PaletteColor;
        if (color == null)
        {
            return false;
        }

        if (values[1] is Avalonia.Media.Color avColor)
        {
            return color.R == avColor.R &&
                   color.G == avColor.G &&
                   color.B == avColor.B;
        }

        return false;
    }
}
