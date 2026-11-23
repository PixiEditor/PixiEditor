using System.Globalization;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.Helpers.Converters;

internal class MeasurementUnitToMinValueConverter : SingleInstanceConverter<MeasurementUnitToMinValueConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is MeasurementConverterData data)
        {
            return data.FromPixels(1, data.GetDpi());
        }

        if(value is int decimals)
        {
            if (decimals > 0)
            {
                return Math.Pow(10, -decimals);
            }

            return 1d;
        }

        return value;
    }
}
