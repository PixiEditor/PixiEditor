using System.Globalization;
using System.Windows.Media;
using PixiEditor.Extensions.Palettes;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

namespace PixiEditor.Helpers.Converters;

internal class BackendColorToMediaColorConverter : SingleInstanceConverter<BackendColorToMediaColorConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        BackendColor backendColor = default;
        if (value is BackendColor)
        {
            backendColor = (BackendColor)value;
        }
        else if (value is PaletteColor paletteColor)
        {
            backendColor = paletteColor.ToColor();
        }

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
