using System.Diagnostics;
using System.Linq;
using System.Text;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Helpers;

namespace PixiEditor.AvaloniaUI.Models.Input;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public record struct KeyCombination(Key Key, KeyModifiers Modifiers)
{
    public static KeyCombination None => new(Key.None, KeyModifiers.None);

    public override string ToString() => ToString(false, false);

    public KeyGesture ToKeyGesture() => new(Key, Modifiers);

    public KeyGesture Gesture => ToKeyGesture();

    private string ToString(bool forceInvariant, bool showNone)
    {
        StringBuilder builder = new();

        foreach (var modifier in Modifiers.GetFlags().OrderByDescending(x => x != KeyModifiers.Alt))
        {
            if (modifier == KeyModifiers.None) continue;

            string key = modifier switch
            {
                KeyModifiers.Control => new LocalizedString("CTRL_KEY"),
                KeyModifiers.Shift => new LocalizedString("SHIFT_KEY"),
                KeyModifiers.Alt => new LocalizedString("ALT_KEY"),
                _ => modifier.ToString()
            };

            builder.Append($"{key}+");
        }

        if (Key != Key.None || showNone)
        {
            builder.Append(InputKeyHelpers.GetKeyboardKey(Key, forceInvariant));
        }

        builder.Append('‎'); // left-to-right marker ensures WPF does not reverse the string when using punctuations as key
        return builder.ToString();
    }

    private string GetDebuggerDisplay() => ToString(true, true);
}
