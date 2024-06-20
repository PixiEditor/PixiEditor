using System.Globalization;
using Avalonia;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class TimelineSliderValueToMarginConverter : SingleInstanceMultiValueConverter<TimelineSliderValueToMarginConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new List<object?> { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3)
        {
            throw new ArgumentException("TimelineSliderValueToMarginConverter requires 3 values");
        }

        if (values[0] is int frame && values[1] is double scale && values[2] is Vector offset)
        {
            return new Thickness(frame * scale - offset.X, 0, 0, 0);
        }

        return new Thickness();
    }
}
