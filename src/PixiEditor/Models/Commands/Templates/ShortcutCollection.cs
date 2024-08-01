using System.Collections.Generic;
using Avalonia.Input;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.Templates;

internal class ShortcutCollection : List<Shortcut>
{
    public ShortcutCollection() { }

    public ShortcutCollection(List<Shortcut> enumerable) : base(enumerable)
    { }

    public void Add(string commandName, Key key, KeyModifiers modifiers)
    {
        Add(new Shortcut(new KeyCombination(key, modifiers), new List<string>() { commandName }));
    }

    /// <summary>
    /// Unassigns a shortcut.
    /// </summary>
    public void Add(string commandName)
    {
        Add(new Shortcut(KeyCombination.None, new List<string> { commandName }));
    }
}
