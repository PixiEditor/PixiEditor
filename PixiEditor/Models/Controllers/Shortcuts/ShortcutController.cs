using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers.Shortcuts
{
    public class ShortcutController
    {
        public ShortcutController(params ShortcutGroup[] shortcutGroups)
        {
            ShortcutGroups = new ObservableCollection<ShortcutGroup>(shortcutGroups);
        }

        public static bool BlockShortcutExecution { get; set; }

        public ObservableCollection<ShortcutGroup> ShortcutGroups { get; init; }

        public Shortcut LastShortcut { get; private set; }

        public const Key MoveViewportToolTransientChangeKey = Key.Space;

        public void KeyPressed(Key key, ModifierKeys modifiers)
        {
            if (!BlockShortcutExecution)
            {
                Shortcut[] shortcuts = ShortcutGroups.SelectMany(x => x.Shortcuts).ToList().FindAll(x => x.ShortcutKey == key).ToArray();
                if (shortcuts.Length < 1)
                {
                    return;
                }

                shortcuts = shortcuts.OrderByDescending(x => x.Modifier).ToArray();
                for (int i = 0; i < shortcuts.Length; i++)
                {
                    if (modifiers.HasFlag(shortcuts[i].Modifier))
                    {
                        shortcuts[i].Execute();
                        LastShortcut = shortcuts[i];
                        break;
                    }
                }
            }
        }
    }
}
