using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding;

public class LayoutElementToNativeControlConverter : IValueConverter
{
    public static LayoutElementToNativeControlConverter Instance { get; } = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ILayoutElement<Control> element)
        {
            return element.BuildNative();
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
