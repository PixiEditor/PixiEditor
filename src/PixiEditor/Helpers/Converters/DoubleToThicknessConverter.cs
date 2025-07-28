using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;

internal class DoubleToThicknessConverter : SingleInstanceConverter<DoubleToThicknessConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            if (parameter is null)
            {
                return new Thickness(doubleValue);
            }

            if (parameter is string str)
            {
                double l = 0, t = 0, r = 0, b = 0;
                if (str.Contains('L'))
                {
                    l = doubleValue;
                }
                if (str.Contains('T'))
                {
                    t = doubleValue;
                }
                if (str.Contains('R'))
                {
                    r = doubleValue;
                }

                if (str.Contains('B'))
                {
                    b = doubleValue;
                }
                
                return new Thickness(l, t, r, b);
            }
        }

        return new Thickness();
    }
}
