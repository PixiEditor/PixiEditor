using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Color = PixiEditor.Extensions.CommonApi.FlyUI.Properties.Color;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class ColorToAvaloniaBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(new Avalonia.Media.Color(color.A, color.R, color.G, color.B));
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
