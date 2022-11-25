using System.Globalization;
using System.Windows.Data;

namespace PixiEditor.Helpers.Converters;

internal class BoolToInverseBoolConverter : SingleInstanceConverter<BoolToInverseBoolConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return Binding.DoNothing;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return Binding.DoNothing;
    }
}
