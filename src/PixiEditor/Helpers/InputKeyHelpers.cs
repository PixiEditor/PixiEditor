using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Helpers;

internal static class InputKeyHelpers
{
    /// <summary>
    /// Returns the character of the <paramref name="key"/> mapped to the users keyboard layout
    /// </summary>
    public static string GetKeyboardKey(Key key, bool forceInvariant = false) =>
        IOperatingSystem.Current.InputKeys.GetKeyboardKey(key, forceInvariant);

    public static bool ModifierUsesSymbol(KeyModifiers modifier)
    {
        return IOperatingSystem.Current.InputKeys.ModifierUsesSymbol(modifier);
    }

    public static Key ModifierToKey(KeyModifiers modifier)
    {
        return modifier switch
        {
            KeyModifiers.Alt => Key.LeftAlt,
            KeyModifiers.Control => Key.LeftCtrl,
            KeyModifiers.Shift => Key.LeftShift,
            KeyModifiers.Meta => Key.LWin,
            _ => Key.None
        };
    }
}
