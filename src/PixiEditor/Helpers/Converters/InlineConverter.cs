using System.Globalization;
using Avalonia.Data.Converters;

namespace PixiEditor.Helpers.Converters;

public class InlineConverter : IValueConverter
{
    Func<object?, bool>? simpleConvert;

    public InlineConverter(Func<object, bool> simpleConvert)
    {
        this.simpleConvert = simpleConvert;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return simpleConvert?.Invoke(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
