using System.Globalization;
using Avalonia.Data.Converters;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class EnumToEnumConverter<T1, T2> : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is T1 enumValue)
        {
            return (T2)(object)enumValue;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is T2 enumValue)
        {
            return (T1)(object)enumValue;
        }

        return null;
    }
}
