using System.Globalization;
using System.Windows;
using PixiEditor.Zoombox;

namespace PixiEditor.Helpers.Converters;

internal class ZoomModeToHitTestVisibleConverter : SingleInstanceConverter<ZoomModeToHitTestVisibleConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ZoomboxMode zoomboxMode)
            return DependencyProperty.UnsetValue;
        return zoomboxMode == ZoomboxMode.Normal;
    }
}
