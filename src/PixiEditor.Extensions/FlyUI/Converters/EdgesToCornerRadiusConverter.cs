using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class EdgesToCornerRadiusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Edges edges)
        {
            return new CornerRadius(edges.Top, edges.Right, edges.Bottom, edges.Left);
        }

        return new CornerRadius();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
