using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;
internal class ReciprocalConverter : SingleInstanceConverter<ReciprocalConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double num)
            return DependencyProperty.UnsetValue;

        double result;
        if (parameter is not double mult)
            result = 1 / num;
        else
            result = mult / num;

        return Math.Clamp(result, 1e-15, 1e15);
    }
}
