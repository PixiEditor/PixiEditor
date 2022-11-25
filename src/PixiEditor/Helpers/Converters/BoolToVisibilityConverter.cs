using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class BoolToVisibilityConverter : SingleInstanceConverter<BoolToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolean = (bool)value;
        return boolean ? Visibility.Visible : Visibility.Collapsed;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
