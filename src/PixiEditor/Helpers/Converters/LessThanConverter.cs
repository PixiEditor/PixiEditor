using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class LessThanConverter : SingleInstanceMultiValueConverter<LessThanConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int left && parameter is int right)
        {
            return left < right;
        }

        return Convert([value], targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 1)
        {
            if (values[0] is int l && parameter is int r)
            {
                return l < r;
            }

            return false;
        }

        if (values.Count != 2)
            throw new ArgumentException("GreaterThanConverter requires exactly two values.");

        if (values[0] is int left && values[1] is int right)
        {
            int adjust = 0;
            if(parameter is int adjustment)
                adjust = adjustment;

            return left < right + adjust;
        }

        return false;
    }
}
