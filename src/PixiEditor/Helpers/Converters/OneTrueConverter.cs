using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class OneTrueConverter : SingleInstanceMultiValueConverter<OneTrueConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<bool> bools)
        {
            return bools.Any(x => x);
        }

        return false;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.All(x => x is bool b))
        {
            return values.Cast<bool>().Any(x => x);
        }

        return false;
    }
}
