using System.Globalization;
using Avalonia.Media;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Helpers.Converters;

internal class SocketColorConverter : SingleInstanceConverter<SocketColorConverter>
{
    static Color unknownColor = Color.FromRgb(255, 0, 255);
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return SocketToColor(value);
    }

    public static Color SocketToColor(object value)
    {
        if (value is IBrush brush)
        {
            if(brush is SolidColorBrush solidColorBrush)
                return solidColorBrush.Color;
            if (brush is GradientBrush linearGradientBrush)
                return linearGradientBrush.GradientStops.FirstOrDefault()?.Color ?? unknownColor;
        }

        return unknownColor;
    }
}
