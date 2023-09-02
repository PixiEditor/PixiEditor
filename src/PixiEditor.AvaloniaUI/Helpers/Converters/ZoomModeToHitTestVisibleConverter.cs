using System.Globalization;
using System.Windows;
using Avalonia;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.Zoombox;

namespace PixiEditor.Helpers.Converters;

internal class ZoomModeToHitTestVisibleConverter : SingleInstanceConverter<ZoomModeToHitTestVisibleConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ZoomboxMode zoomboxMode)
            return AvaloniaProperty.UnsetValue;
        return zoomboxMode == ZoomboxMode.Normal;
    }
}
