using System.Globalization;
using Avalonia;
using PixiEditor.Zoombox;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class ZoomModeToHitTestVisibleConverter : SingleInstanceConverter<ZoomModeToHitTestVisibleConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ZoomboxMode zoomboxMode)
            return AvaloniaProperty.UnsetValue;
        return zoomboxMode == ZoomboxMode.Normal;
    }
}
