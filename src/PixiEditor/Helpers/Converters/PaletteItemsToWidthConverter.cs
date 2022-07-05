using System.Globalization;
using SkiaSharp;

namespace PixiEditor.Helpers.Converters;

internal class PaletteItemsToWidthConverter : SingleInstanceConverter<PaletteItemsToWidthConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IList<SKColor> colors && colors.Count == 0)
        {
            return 0;
        }

        return 120;
    }
}
