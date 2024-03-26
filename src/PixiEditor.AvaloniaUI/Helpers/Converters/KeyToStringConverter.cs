using System.Globalization;
using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Helpers.Converters;

internal class KeyToStringConverter
    : SingleInstanceConverter<KeyToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            Key key => (object)InputKeyHelpers.GetKeyboardKey(key),
            KeyModifiers modifier => modifier switch
            {
                KeyModifiers.Control => new LocalizedString("CTRL_KEY"),
                KeyModifiers.Shift => new LocalizedString("SHIFT_KEY"),
                KeyModifiers.Alt => new LocalizedString("ALT_KEY"),
                _ => modifier.ToString()
            },
            _ => string.Empty
        };
}
