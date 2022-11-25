using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class RadiansToDegreesConverter : SingleInstanceConverter<RadiansToDegreesConverter>
{
    private const double RadiansToDegrees = 180 / Math.PI;

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        if (value is double angle)
        {
            return Math.Truncate((angle * RadiansToDegrees + 360) % 360);
        }

        return 0;
    }
}
