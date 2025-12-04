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

    public static bool TryGetModifierFromString(string part, out KeyModifiers modifiers)
    {
        modifiers = KeyModifiers.None;
        if (string.IsNullOrWhiteSpace(part)) return false;

        if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("Control", StringComparison.OrdinalIgnoreCase))
        {
            modifiers = KeyModifiers.Control;
            return true;
        }

        if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
        {
            modifiers = KeyModifiers.Shift;
            return true;
        }

        if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
        {
            modifiers = KeyModifiers.Alt;
            return true;
        }

        if (part.Equals("Meta", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("Super", StringComparison.OrdinalIgnoreCase))
        {
            modifiers = KeyModifiers.Meta;
            return true;
        }

        return false;
    }

    public static bool TryGetKeyFromString(string part, out Key key)
    {
        key = Key.None;
        if (string.IsNullOrWhiteSpace(part)) return false;

        // Try parse as enum first
        if (Enum.TryParse(part, true, out Key parsedKey))
        {
            key = parsedKey;
            return true;
        }

        // Try parse as character
        if (part.Length == 1)
        {
            char c = part[0];
            if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c))
            {
                key = (Key)char.ToUpper(c);
                return true;
            }
        }

        return false;
    }
}
