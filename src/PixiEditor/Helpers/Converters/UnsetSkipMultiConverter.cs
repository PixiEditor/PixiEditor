using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class UnsetSkipMultiConverter : SingleInstanceMultiValueConverter<UnsetSkipMultiConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value is not UnsetValueType)
                return value;
        }

        return AvaloniaProperty.UnsetValue;
    }
}
