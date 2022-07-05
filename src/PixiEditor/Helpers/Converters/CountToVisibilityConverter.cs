using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class CountToVisibilityConverter : SingleInstanceConverter<CountToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intVal)
        {
            return intVal == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Visible;
    }
}
