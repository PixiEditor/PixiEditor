using System.Globalization;
using System.Text;
using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Models.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Helpers.Converters;

internal class KeyToStringConverter
    : SingleInstanceConverter<KeyToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            Key key => ConvertKey(key),
            KeyModifiers modifier => ConvertModifier(modifier),
            KeyGesture gesture => ConvertKeyCombination(gesture),
            _ => string.Empty
        };
    }

    private static string ConvertKey(Key key)
    {
        return InputKeyHelpers.GetKeyboardKey(key);
    }

    private static string ConvertModifier(KeyModifiers modifier)
    {
        if (IOperatingSystem.Current.InputKeys.ModifierUsesSymbol(modifier))
        {
            return InputKeyHelpers.GetKeyboardKey(InputKeyHelpers.ModifierToKey(modifier));
        }

        return modifier switch
        {
            KeyModifiers.Control => "CTRL_KEY",
            KeyModifiers.Shift => "SHIFT_KEY",
            KeyModifiers.Alt => "ALT_KEY",
            _ => modifier.ToString()
        };
    }

    private string ConvertKeyCombination(KeyGesture value)
    {
        var flags = value.KeyModifiers.GetFlags().OrderByDescending(x => x != KeyModifiers.Alt);
        var builder = new StringBuilder();
        
        foreach (var modifier in flags)
        {
            if (modifier == KeyModifiers.None) continue;

            string mod = ConvertModifier(modifier);

            builder.Append($"{mod}+");
        }
        
        if (value.Key != Key.None)
        {
            builder.Append(ConvertKey(value.Key));
        }
        
        builder.Append('‎'); // left-to-right marker ensures Avalonia does not reverse the string when using punctuations as key
        return builder.ToString();
    }
}
