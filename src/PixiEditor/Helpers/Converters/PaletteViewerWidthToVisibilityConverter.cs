using System.Globalization;
using System.Windows;

namespace PixiEditor.Helpers.Converters;

internal class PaletteViewerWidthToVisibilityConverter : SingleInstanceConverter<PaletteViewerWidthToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isCompact = value is double and < 100;
        if (parameter is string and "Hidden")
        {
            return isCompact ? Visibility.Hidden : Visibility.Visible;
        }

        return isCompact ? Visibility.Visible : Visibility.Hidden;
    }
}
