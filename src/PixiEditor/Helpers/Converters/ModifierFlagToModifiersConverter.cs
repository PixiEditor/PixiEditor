using System.Collections.Generic;
using System.Globalization;
using Avalonia.Input;

namespace PixiEditor.Helpers.Converters;

internal class ModifierFlagToModifiersConverter : SingleInstanceConverter<ModifierFlagToModifiersConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return GetModifiers((KeyModifiers)value);
    }

    private IEnumerable<KeyModifiers> GetModifiers(KeyModifiers keys)
    {
        if (keys.HasFlag(KeyModifiers.Meta))
        {
            yield return KeyModifiers.Meta;
        }
        else if (keys.HasFlag(KeyModifiers.Control))
        {
            yield return KeyModifiers.Control;
        }
        else if (keys.HasFlag(KeyModifiers.Shift))
        {
            yield return KeyModifiers.Shift;
        }
        else if (keys.HasFlag(KeyModifiers.Alt))
        {
            yield return KeyModifiers.Alt;
        }
    }
}
