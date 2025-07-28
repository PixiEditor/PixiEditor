using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class IconLookupConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string iconName)
        {
            return null;
        }

        if (Application.Current.Styles.TryGetResource(iconName, null, out object resource))
        {
            return resource;
        }

        return iconName;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
