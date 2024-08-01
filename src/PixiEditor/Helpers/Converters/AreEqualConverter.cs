using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class AreEqualConverter : SingleInstanceMultiValueConverter<AreEqualConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(new[] { value }, targetType, parameter, culture);
    }

    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return false;
        }

        for (int i = 1; i < values.Count; i++)
        {
            if(values[i] == null || values[i - 1] == null)
            {
                return false;
            }
            
            if (!values[i].Equals(values[i - 1]))
            {
                return false;
            }
        }

        return true;
    }
}
