using System.Collections.Generic;
using System.Globalization;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Helpers.Converters;

internal class IndexToAssociatedKeyConverter : SingleInstanceMultiValueConverter<IndexToAssociatedKeyConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new List<object?> { value, parameter }, targetType, null, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2) return (int?)null;

        if (values[0] is not PaletteColor paletteColor) return null;
        if(values[1] is not IList<PaletteColor> paletteColors) return null;

        int colIndex = paletteColors.IndexOf(paletteColor);

        if (colIndex == -1) return null;
        if (colIndex < 10)
        {
            if (colIndex == 9) return 0;
            return (int?)colIndex + 1;
        }

        return null;
    }
}
