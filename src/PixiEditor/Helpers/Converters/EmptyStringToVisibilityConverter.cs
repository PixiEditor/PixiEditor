using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class EmptyStringToVisibilityConverter :
    SingleInstanceConverter<EmptyStringToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return
            string.IsNullOrEmpty((string)value)
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}
