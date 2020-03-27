using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers
{
    public class ShortcutController
    {
        public static bool BlockShortcutExecution { get; set; }

        public List<Shortcut> Shortcuts { get; set; }       

        public ShortcutController()
        {
            Shortcuts = new List<Shortcut>();
            
        }

        public void KeyPressed(Key key)
        {
            if (!BlockShortcutExecution)
            {
                Shortcut shortcut = Shortcuts.Find(x => x.ShortcutKey == key);
                if (shortcut == null) return;
                if (Keyboard.Modifiers.HasFlag(shortcut.Modifier))
                {
                    shortcut.Execute();
                }
            }
        }
    }
}
