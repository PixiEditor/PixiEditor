using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Input;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public record struct KeyCombination(Key Key, ModifierKeys Modifiers)
    {
        public static KeyCombination None => new(Key.None, ModifierKeys.None);

        public override string ToString() => ToString(CultureInfo.CurrentCulture);

        public string ToString(CultureInfo culture)
        {
            StringBuilder builder = new();

            foreach (ModifierKeys modifier in Modifiers.GetFlags().OrderByDescending(x => x != ModifierKeys.Alt))
            {
                if (modifier == ModifierKeys.None) continue;

                string key = modifier switch
                {
                    ModifierKeys.Control => "Ctrl",
                    _ => modifier.ToString()
                };

                builder.Append($"{key}+");
            }

            if (Key != Key.None)
            {
                builder.Append(InputKeyHelpers.GetKeyboardKey(Key, culture));
            }

            return builder.ToString();
        }

        private string GetDebuggerDisplay() => ToString(CultureInfo.InvariantCulture);
    }
}
