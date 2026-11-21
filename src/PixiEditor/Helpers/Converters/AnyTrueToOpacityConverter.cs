using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class AnyTrueToOpacityConverter : SingleInstanceMultiValueConverter<AnyTrueToOpacityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<bool> bools)
        {
            return bools.Any(x => x) ? 1d : 0d;
        }

        return 0d;
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.All(x => x is bool b))
        {
            return values.Cast<bool>().Any(x => x) ? 1d : 0d;
        }

        return 0d;
    }
}
