using System.Globalization;
using Avalonia;

namespace PixiEditor.Helpers.Converters;
internal class ReciprocalConverter : SingleInstanceConverter<ReciprocalConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double num)
            return AvaloniaProperty.UnsetValue;

        return Convert(num, parameter is double mult ? mult : 1d);
    }

    public static double Convert(double num, double mult = 1)
    {
        var result = mult / num;
        return Math.Clamp(result, 1e-15, 1e15);
    }
}
