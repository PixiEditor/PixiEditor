using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class MultiSubtractConverter : SingleInstanceMultiValueConverter<MultiSubtractConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count == 0)
        {
            return 0;
        }
        
        if (values.Count < 2)
        {
            return values[0];
        }
        
        if (values[0] is not double)
        {
            return 0;
        }

        double result = (double)values[0];
        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] is double doubleVal)
            {
                result -= doubleVal;
            }
            else if (values[i] is Thickness thickness)
            {
                result -= thickness.Left + thickness.Right + thickness.Top + thickness.Bottom;
            }
        }

        return result;
    }
}
