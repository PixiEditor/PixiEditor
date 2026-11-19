using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class DecimalsToMinValueConverter : SingleInstanceConverter<DecimalsToMinValueConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is int decimals)
        {
            if (decimals > 0)
            {
                return Math.Pow(10, -decimals);
            }

            return 1;
        }

        return value;
    }
}
