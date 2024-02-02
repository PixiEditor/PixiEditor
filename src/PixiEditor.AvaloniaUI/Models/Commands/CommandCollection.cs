using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Input;
using PixiEditor.AvaloniaUI.Models.Structures;
using Command = PixiEditor.AvaloniaUI.Models.Commands.Commands.Command;

namespace PixiEditor.AvaloniaUI.Models.Commands;

[DebuggerDisplay("Count = {Count}")]
internal class CommandCollection : ICollection<Commands.Command>
{
    private readonly Dictionary<string, Command> commandInternalNames;
    private readonly OneToManyDictionary<KeyCombination, Command> _commandShortcuts;

    public int Count => commandInternalNames.Count;

    public bool IsReadOnly => false;

    public Command this[string name] => commandInternalNames[name];
    public bool ContainsKey(string key) => commandInternalNames.ContainsKey(key);

    public List<Command> this[KeyCombination shortcut] => _commandShortcuts[shortcut];

    public CommandCollection()
    {
        commandInternalNames = new();
        _commandShortcuts = new();
    }

    public void Add(Command item)
    {
        commandInternalNames.Add(item.InternalName, item);
        _commandShortcuts.Add(item.Shortcut, item);
    }

    public void Clear()
    {
        commandInternalNames.Clear();
        _commandShortcuts.Clear();
    }

    public void ClearShortcuts() => _commandShortcuts.Clear();

    public bool Contains(Command item) => commandInternalNames.ContainsKey(item.InternalName);

    public void CopyTo(Command[] array, int arrayIndex) => commandInternalNames.Values.CopyTo(array, arrayIndex);

    public IEnumerator<Command> GetEnumerator() => commandInternalNames.Values.GetEnumerator();

    public bool Remove(Command item)
    {
        bool anyRemoved = false;

        anyRemoved |= commandInternalNames.Remove(item.InternalName);
        anyRemoved |= _commandShortcuts.Remove(item);

        return anyRemoved;
    }

    public void AddShortcut(Command command, KeyCombination shortcut)
    {
        _commandShortcuts.Remove(KeyCombination.None, command);
        _commandShortcuts.Add(shortcut, command);
    }

    public void RemoveShortcut(Command command, KeyCombination shortcut)
    {
        _commandShortcuts.Remove(shortcut, command);
        _commandShortcuts.Add(KeyCombination.None, command);
    }

    public void ClearShortcut(KeyCombination shortcut)
    {
        if (shortcut is { Key: Key.None, Modifiers: KeyModifiers.None })
            return;
        _commandShortcuts.AddRange(KeyCombination.None, _commandShortcuts[shortcut]);
        _commandShortcuts.Clear(shortcut);
    }

    public IEnumerable<KeyValuePair<KeyCombination, IEnumerable<Command>>> GetShortcuts() =>
        _commandShortcuts;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
