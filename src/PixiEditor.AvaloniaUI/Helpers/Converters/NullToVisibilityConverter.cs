using System.Globalization;
using PixiEditor.UI.Common.Converters;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class NullToVisibilityConverter
    : SingleInstanceConverter<NullToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null;
    }
}
