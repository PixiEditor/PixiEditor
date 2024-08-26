using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class AllTrueConverter : SingleInstanceMultiValueConverter<AllTrueConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<bool> bools)
        {
            return bools.All(x => x);
        }

        return false;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.All(x => x is bool b))
        {
            return values.Cast<bool>().All(x => x);
        }

        return false;
    }
}
