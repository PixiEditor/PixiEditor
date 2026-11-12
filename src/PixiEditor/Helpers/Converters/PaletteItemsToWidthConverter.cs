using System.Globalization;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Helpers.Converters;

internal class PaletteItemsToWidthConverter : SingleInstanceConverter<PaletteItemsToWidthConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IList<PaletteColor> { Count: > 0 })
        {
            return 60;
        }

        return 0;
    }
}
