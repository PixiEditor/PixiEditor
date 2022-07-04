using System;
using System.Globalization;

namespace PixiEditor.Helpers.Converters;

public class DebugConverter
    : SingleInstanceConverter<DebugConverter>
{
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}