using System.Globalization;
using Avalonia;
using PixiEditor.AvaloniaUI.Helpers.Converters;

namespace PixiEditor.Helpers.Converters;

internal class ZoomToViewportConverter
    : SingleInstanceConverter<ZoomToViewportConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double scale && parameter is double factor)
        {
            double newSize = Math.Clamp((double)factor / scale, 1, 9999);
            if (newSize > 1 && newSize < 4)
                newSize = 4;
            return new Rect(0, 0, newSize, newSize);
        }

        return AvaloniaProperty.UnsetValue;
    }
}
