using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class OppositeVisibilityConverter
    : SingleInstanceConverter<OppositeVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value.ToString().ToLower() == "visible")
        {
            return Visibility.Hidden;
        }

        return Visibility.Visible;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible ? "Hidden" : "Visible";
        }

        return null;
    }
}
