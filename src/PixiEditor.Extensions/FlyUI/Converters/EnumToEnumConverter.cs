using System.Globalization;
using Avalonia.Data.Converters;

namespace PixiEditor.Extensions.FlyUI.Converters;

public class EnumToEnumConverter<T1, T2> : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is T1 enumValue)
        {
            int enumInt = (int)(object)enumValue;
            return Enum.ToObject(typeof(T2), enumInt);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is T2 enumValue)
        {
            int enumInt = (int)(object)enumValue;
            return Enum.ToObject(typeof(T1), enumInt);
        }

        return null;
    }
}
