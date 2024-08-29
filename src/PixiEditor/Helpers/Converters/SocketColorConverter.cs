using System.Globalization;
using Avalonia.Media;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;

namespace PixiEditor.Helpers.Converters;

internal class SocketColorConverter : SingleInstanceConverter<SocketColorConverter>
{
    Color unknownColor = Color.FromRgb(255, 0, 255);
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
