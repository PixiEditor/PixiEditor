using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class ZoomToViewportConverter
    : SingleInstanceConverter<ZoomToViewportConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double scale && parameter is double factor)
        {
            var newSize = ZoomToViewport(factor, scale);
            return new RelativeRect(0, 0, newSize, newSize, RelativeUnit.Absolute);
        }

        return AvaloniaProperty.UnsetValue;
    }

    public static double ZoomToViewport(double factor, double scale)
    {
        double newSize = Math.Clamp(factor / scale, 1, 9999);

        double log = Math.Log(newSize, 2);
        //round to power of 2
        newSize = Math.Pow(2, Math.Round(log));
        return newSize;
    }
}
