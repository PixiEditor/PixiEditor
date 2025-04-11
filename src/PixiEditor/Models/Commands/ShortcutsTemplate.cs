using System.Collections.Generic;
using Avalonia.Input;
using PixiEditor.Models.Commands.Templates.Providers.Parsers;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands;

[Serializable]
public sealed class ShortcutsTemplate
{
    public List<Shortcut> Shortcuts { get; set; }

    public ShortcutsTemplate()
    {
        Shortcuts = new List<Shortcut>();
    }

    public static ShortcutsTemplate FromKeyDefinitions(List<KeyDefinition> keyDefinitions)
    {
        ShortcutsTemplate template = new ShortcutsTemplate();
        foreach (KeyDefinition keyDefinition in keyDefinitions)
        {
            foreach (string command in keyDefinition.Commands)
            {
                template.Shortcuts.Add(new Shortcut(keyDefinition.DefaultShortcut.ToKeyCombination(), command));
            }
        }

        return template;
    }
}

[Serializable]
public sealed class Shortcut
{
    public KeyCombination KeyCombination { get; set; }
    public List<string> Commands { get; set; }
    
    public Shortcut() { }

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
    
    public Shortcut(Key key, KeyModifiers KeyModifiers, string command)
    {
        KeyCombination = new KeyCombination(key, KeyModifiers);
        Commands = new List<string> { command };
    }
}
