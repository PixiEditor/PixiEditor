using System.Globalization;
using Avalonia;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class ZoomToViewportConverter
    : SingleInstanceConverter<ZoomToViewportConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double scale && parameter is double factor)
        {
            double newSize = Math.Clamp(factor / scale, 2, 9999);

            //round to power of 2
            newSize = Math.Pow(2, Math.Round(Math.Log(newSize, 2)));
            return new RelativeRect(0, 0, newSize, newSize, RelativeUnit.Absolute);
        }

        return AvaloniaProperty.UnsetValue;
    }
}
