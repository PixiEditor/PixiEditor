using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class TimelineSliderWidthToMaximumConverter : SingleInstanceMultiValueConverter<TimelineSliderWidthToMaximumConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new List<object?> { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3)
        {
            throw new ArgumentException("TimelineSliderWidthToMaximumConverter requires 2 values");
        }

        if (values[0] is Rect bounds && values[1] is double scale && values[2] is Vector offset)
        {
            return Math.Floor((bounds.Width + offset.X) / scale);
        }

        return 0;
    }
}
