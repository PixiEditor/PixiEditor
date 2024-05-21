using System.Globalization;
using Avalonia;
using Avalonia.Controls.Converters;
using Avalonia.Input;

namespace PixiEditor.UI.Common.Converters;

internal class MenuItemKeyGestureConverter : SingleInstanceConverter<MenuItemKeyGestureConverter>
{
    PlatformKeyGestureConverter converter = new();

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is KeyGesture { Key: Key.None, KeyModifiers: KeyModifiers.None })
            return string.Empty;

        return converter.Convert(value, targetType, parameter, culture);
    }
}
