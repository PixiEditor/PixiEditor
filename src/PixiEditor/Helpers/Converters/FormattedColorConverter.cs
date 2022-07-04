using System;
using System.Globalization;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters;

public class FormattedColorConverter
    : SingleInstanceMultiValueConverter<FormattedColorConverter>
{
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null ||
            values.Length <= 1 ||
            values[0] is not Color color ||
            values[1] is not string format)
        {
            return "";
        }

        return format.ToLowerInvariant() switch
        {
            "hex" => color.ToString(),
            "rgba" => $"({color.R}, {color.G}, {color.B}, {color.A})",
            _ => "",
        };
    }
}