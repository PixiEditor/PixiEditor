using System.Globalization;
using System.Windows.Input;

namespace PixiEditor.Helpers.Converters;

public class ModifierFlagToModifiersConverter : SingleInstanceConverter<ModifierFlagToModifiersConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return GetModifiers((ModifierKeys)value);
    }
    
    private IEnumerable<ModifierKeys> GetModifiers(ModifierKeys keys)
    {
        if (keys.HasFlag(ModifierKeys.Windows))
        {
            yield return ModifierKeys.Windows;
        }
        else if (keys.HasFlag(ModifierKeys.Control))
        {
            yield return ModifierKeys.Control;
        }
        else if (keys.HasFlag(ModifierKeys.Shift))
        {
            yield return ModifierKeys.Shift;
        }
        else if (keys.HasFlag(ModifierKeys.Alt))
        {
            yield return ModifierKeys.Alt;
        }
    }
}