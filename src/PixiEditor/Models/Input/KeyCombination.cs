using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Avalonia.Input;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Helpers;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Input;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public record struct KeyCombination(Key Key, KeyModifiers Modifiers)
{
    public static KeyCombination None => new(Key.None, KeyModifiers.None);

    public override string ToString() => ToString(false, false);

    public KeyGesture ToKeyGesture() => new(Key, Modifiers);

    [JsonIgnore]
    public KeyGesture Gesture => ToKeyGesture();

    public static KeyCombination TryParse(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return None;

        var parts = s.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return None;

        KeyModifiers modifiers = KeyModifiers.None;
        Key key = Key.None;

        foreach (var part in parts)
        {
            if (InputKeyHelpers.TryGetModifierFromString(part, out var modifier))
            {
                modifiers |= modifier;
            }
            else if (InputKeyHelpers.TryGetKeyFromString(part, out var parsedKey))
            {
                key = parsedKey;
            }
            else
            {
                return None; // Invalid part
            }
        }

        return new KeyCombination(key, modifiers);
    }

    private string ToString(bool forceInvariant, bool showNone)
    {
        StringBuilder builder = new();

        foreach (var modifier in Modifiers.GetFlags().OrderByDescending(x => x != KeyModifiers.Alt))
        {
            if (modifier == KeyModifiers.None) continue;

            string key;
            if (InputKeyHelpers.ModifierUsesSymbol(modifier))
            {
                key = InputKeyHelpers.GetKeyboardKey(InputKeyHelpers.ModifierToKey(modifier), forceInvariant);
            }
            else
            {
                key = modifier switch
                {
                    KeyModifiers.Control => new LocalizedString("CTRL_KEY"),
                    KeyModifiers.Shift => new LocalizedString("SHIFT_KEY"),
                    KeyModifiers.Alt => new LocalizedString("ALT_KEY"),
                    _ => modifier.ToString()
                };
            }

            builder.Append($"{key}+");
        }
        
        if (Key != Key.None || showNone)
        {
            builder.Append(InputKeyHelpers.GetKeyboardKey(Key, forceInvariant));
        }

        builder.Append('‎'); // left-to-right marker ensures Avalonia does not reverse the string when using punctuations as key
        return builder.ToString();
    }

    private string GetDebuggerDisplay() => ToString(true, true);
}
