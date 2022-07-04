using PixiEditor.Models.DataHolders;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using OneOf.Types;

namespace PixiEditor.Models.Commands;

[DebuggerDisplay("Count = {Count}")]
public class CommandCollection : ICollection<Command>
{
    private readonly Dictionary<string, Command> _commandInternalNames;
    private readonly OneToManyDictionary<KeyCombination, Command> _commandShortcuts;

    public int Count => _commandInternalNames.Count;

    public bool IsReadOnly => false;

    public Command this[string name] => _commandInternalNames[name];

    public IEnumerable<Command> this[KeyCombination shortcut] => _commandShortcuts[shortcut];

    public CommandCollection()
    {
        _commandInternalNames = new();
        _commandShortcuts = new();
    }

    public void Add(Command item)
    {
        _commandInternalNames.Add(item.InternalName, item);
        _commandShortcuts.Add(item.Shortcut, item);
    }

    public void Clear()
    {
        _commandInternalNames.Clear();
        _commandShortcuts.Clear();
    }

    public void ClearShortcuts() => _commandShortcuts.Clear();

    public bool Contains(Command item) => _commandInternalNames.ContainsKey(item.InternalName);

    public void CopyTo(Command[] array, int arrayIndex) => _commandInternalNames.Values.CopyTo(array, arrayIndex);

    public IEnumerator<Command> GetEnumerator() => _commandInternalNames.Values.GetEnumerator();

    public bool Remove(Command item)
    {
        bool anyRemoved = false;

        anyRemoved |= _commandInternalNames.Remove(item.InternalName);
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
        if (shortcut is { Key: Key.None, Modifiers: ModifierKeys.None })
            return;
        _commandShortcuts.AddRange(KeyCombination.None, _commandShortcuts[shortcut]);
        _commandShortcuts.Clear(shortcut);      
    }

    public IEnumerable<KeyValuePair<KeyCombination, IEnumerable<Command>>> GetShortcuts() =>
        _commandShortcuts;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}