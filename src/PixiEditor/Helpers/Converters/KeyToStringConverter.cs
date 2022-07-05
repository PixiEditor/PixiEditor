using System.Globalization;
using System.Windows.Input;

namespace PixiEditor.Helpers.Converters;

internal class KeyToStringConverter
    : SingleInstanceConverter<KeyToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Key key)
        {
            return InputKeyHelpers.GetKeyboardKey(key);
        }
        else if (value is ModifierKeys)
        {
            return value.ToString();
        }
        else
        {
            return string.Empty;
        }
    }
}
