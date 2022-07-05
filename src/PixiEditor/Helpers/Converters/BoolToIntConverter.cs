using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class BoolToIntConverter
    : SingleInstanceConverter<BoolToIntConverter>
{
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString() == "0";
    }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            if (boolean == false)
            {
                return 0;
            }
        }

        return 1;
    }
}
