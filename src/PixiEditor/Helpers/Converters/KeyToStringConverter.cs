using System.Globalization;
using System.Windows.Input;
using PixiEditor.Localization;

namespace PixiEditor.Helpers.Converters;

internal class KeyToStringConverter
    : SingleInstanceConverter<KeyToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            Key key => (object)InputKeyHelpers.GetKeyboardKey(key),
            ModifierKeys modifier => modifier switch
            {
                ModifierKeys.Control => new LocalizedString("CTRL_KEY"),
                ModifierKeys.Shift => new LocalizedString("SHIFT_KEY"),
                ModifierKeys.Alt => new LocalizedString("ALT_KEY"),
                _ => modifier.ToString()
            },
            _ => string.Empty
        };
}
