using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Helpers.Converters;

internal class FormattedColorConverter
    : SingleInstanceMultiValueConverter<FormattedColorConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new[] { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null ||
            values.Count <= 1 ||
            values[0] is not Color color ||
            values[1] is not string format)
        {
            return "";
        }

        return format.ToLowerInvariant() switch
        {
            "hex" => color.ToColor().ToRgbHex(),
            "rgba" => $"({color.R}, {color.G}, {color.B}, {color.A})",
            _ => "",
        };
    }
}
