using System.Globalization;
using Avalonia;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class OffsetToNegatingMarginConverter : SingleInstanceConverter<OffsetToNegatingMarginConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Vector offset)
        {
            return new Thickness(offset.X, offset.Y, 0, 0);
        }

        return new Thickness();
    }
}
