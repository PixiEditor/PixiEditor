using System.Collections.Generic;
using System.Globalization;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class PaletteItemsToWidthConverter : SingleInstanceConverter<PaletteItemsToWidthConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IList<Color> colors && colors.Count == 0)
        {
            return 0;
        }

        return 120;
    }
}
