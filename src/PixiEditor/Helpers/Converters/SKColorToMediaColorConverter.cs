using System.Globalization;
using System.Windows.Media;
using SkiaSharp;

namespace PixiEditor.Helpers.Converters;

internal class SKColorToMediaColorConverter : SingleInstanceConverter<SKColorToMediaColorConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var skcolor = (SKColor)value;
        var color = Color.FromArgb(skcolor.Alpha, skcolor.Red, skcolor.Green, skcolor.Blue);

        if (targetType == typeof(Brush))
        {
            return new SolidColorBrush(color);
        }
        else
        {
            return color;
        }
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = (Color)value;
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}
