using Avalonia.Input;
using System.Collections.Generic;

namespace AvaloniaPlayground.Models
{
    public static class Keyboard
    {
        public static HashSet<Key> Keys { get; set; } = new HashSet<Key>();
        public static KeyModifiers Modifiers { get; private set; } = KeyModifiers.None;

        public static void AddKeyPressed(Key key)
        {
            if (!Keys.Contains(key))
            {
                Keys.Add(key);
            }
        }

        public static void RemoveKeyPressed(Key key)
        {
            if (Keys.Contains(key))
            {
                Keys.Remove(key);
            }
        }

        public static bool IsKeyPressed(Key key)
        {
            return Keys.Contains(key);
        }

        public static void AddKeyModifiersPressed(KeyModifiers modifiers)
        {
            if (!Modifiers.HasFlag(modifiers))
            {
                Modifiers |= modifiers;
            }
        }

        public static void RemoveModifierPressed(KeyModifiers modifiers)
        {
            if (Modifiers.HasFlag(modifiers))
            {
                Modifiers &= ~modifiers;
            }
        }
    }
}
