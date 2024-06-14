using System.Globalization;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class DurationToWidthConverter : SingleInstanceMultiValueConverter<DurationToWidthConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new List<object?> { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
        {
            throw new ArgumentException("DurationToWidthConverter requires 2 values");
        }
        
        if(values[0] is int duration && values[1] is double scale)
        {
            return duration * scale;
        }
        
        return 0;
    }
}
