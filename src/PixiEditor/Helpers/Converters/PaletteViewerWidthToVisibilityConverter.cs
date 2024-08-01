using System.Globalization;

namespace PixiEditor.Helpers.Converters;

internal class PaletteViewerWidthToVisibilityConverter : SingleInstanceConverter<PaletteViewerWidthToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isCompact = value is double and < 100;
        if (parameter is "False" or false)
        {
            return !isCompact;
        }

        return isCompact;
    }
}
