using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;
internal class ReciprocalConverter : SingleInstanceConverter<ReciprocalConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double num)
            return DependencyProperty.UnsetValue;
        if (parameter is not double mult)
            return 1 / num;
        return mult / num;
    }
}
