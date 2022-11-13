using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class BoolToHiddenVisibilityConverter : SingleInstanceConverter<BoolToHiddenVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolean = (bool)value;
        return boolean ? Visibility.Visible : Visibility.Hidden;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
