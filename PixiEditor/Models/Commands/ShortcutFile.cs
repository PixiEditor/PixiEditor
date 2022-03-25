using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PixiEditor.Models.Commands
{
    public class ShortcutFile
    {
        public string Path { get; }

        public ShortcutFile(string path)
        {
            Path = path;
        }

        public void SaveShortcut(string name, Key key, ModifierKeys modifiers)
        {

        }
    }
}
