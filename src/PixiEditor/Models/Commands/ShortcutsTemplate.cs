using System.Drawing;
using System.IO;
using System.Windows.Input;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Commands;

[Serializable]
public sealed class ShortcutsTemplate
{
    public List<Shortcut> Shortcuts { get; set; }

    public ShortcutsTemplate()
    {
        Shortcuts = new List<Shortcut>();
    }
}

[Serializable]
public sealed class Shortcut
{
    public KeyCombination KeyCombination { get; set; }
    public List<string> Commands { get; set; }
    
    public Shortcut() {}

    public Shortcut(KeyCombination keyCombination, List<string> commands)
    {
        KeyCombination = keyCombination;
        Commands = commands;
    }
    
    public Shortcut(KeyCombination keyCombination, string command)
    {
        KeyCombination = keyCombination;
        Commands = new List<string> { command };
    }
    
    public Shortcut(Key key, ModifierKeys modifierKeys, string command)
    {
        KeyCombination = new KeyCombination(key, modifierKeys);
        Commands = new List<string> { command };
    }
}
