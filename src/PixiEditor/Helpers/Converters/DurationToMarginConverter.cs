using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class DurationToMarginConverter : SingleInstanceMultiValueConverter<DurationToMarginConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new List<object?> { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3)
        {
            return 0;
            throw new ArgumentException("DurationToWidthConverter requires 2 values");
        }
        
        if(values[0] is int startFrame && values[1] is double min && values[2] is double scale)
        {
            return new Thickness((startFrame - min) * scale, 0, 0, 0);
        }
        
        return 0;
    }
}
