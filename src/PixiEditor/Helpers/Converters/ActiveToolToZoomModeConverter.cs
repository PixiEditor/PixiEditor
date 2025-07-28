using System.Globalization;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.Zoombox;

namespace PixiEditor.Helpers.Converters;
internal class ActiveToolToZoomModeConverter : SingleInstanceConverter<ActiveToolToZoomModeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            MoveViewportToolViewModel => ZoomboxMode.Move,
            ZoomToolViewModel => ZoomboxMode.Zoom,
            RotateViewportToolViewModel => ZoomboxMode.Rotate,
            _ => ZoomboxMode.Normal,
        };
    }
}
