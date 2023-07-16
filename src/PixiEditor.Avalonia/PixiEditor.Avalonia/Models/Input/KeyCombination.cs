using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Models.Localization;

namespace PixiEditor.Models.DataHolders;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public record struct KeyCombination(Key Key, KeyModifiers Modifiers)
{
    public static KeyCombination None => new(Key.None, KeyModifiers.None);

    public override string ToString() => ToString(false);

    public KeyGesture ToKeyGesture() => new(Key, Modifiers);

    private string ToString(bool forceInvariant)
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

        if (Key != Key.None)
        {
            builder.Append(InputKeyHelpers.GetKeyboardKey(Key, forceInvariant));
        }

        builder.Append('‎'); // left-to-right marker ensures WPF does not reverse the string when using punctuations as key
        return builder.ToString();
    }

    private string GetDebuggerDisplay() => ToString(true);
}
