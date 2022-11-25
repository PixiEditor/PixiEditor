using System.Globalization;
using System.Windows.Media;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

namespace PixiEditor.Helpers.Converters;

internal class BackendColorToMediaColorConverter : SingleInstanceConverter<BackendColorToMediaColorConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var backendColor = (BackendColor)value;
        var color = Color.FromArgb(backendColor.A, backendColor.R, backendColor.G, backendColor.B);

        if (targetType == typeof(Brush))
        {
            return new SolidColorBrush(color);
        }

        return color;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = (Color)value;
        return new BackendColor(color.R, color.G, color.B, color.A);
    }
}
