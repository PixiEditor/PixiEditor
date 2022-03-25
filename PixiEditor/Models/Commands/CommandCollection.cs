using PixiEditor.Models.DataHolders;
using System.Collections;
using System.Diagnostics;

namespace PixiEditor.Models.Commands
{
    [DebuggerDisplay("Count = {Count}")]
    public class CommandCollection : ICollection<Command>
    {
        private readonly Dictionary<string, Command> _commandNames;
        private readonly EnumerableDictionary<KeyCombination, Command> _commandShortcuts;

        public int Count => _commandNames.Count;

        public bool IsReadOnly => false;

        public Command this[string name] => _commandNames[name];

        public IEnumerable<Command> this[KeyCombination shortcut] => _commandShortcuts[shortcut];

        public CommandCollection()
        {
            _commandNames = new();
            _commandShortcuts = new();
        }

        public void Add(Command item)
        {
            _commandNames.Add(item.Name, item);
            _commandShortcuts.Add(item.Shortcut, item);
        }

        public void Clear()
        {
            _commandNames.Clear();
            _commandShortcuts.Clear();
        }

        public bool Contains(Command item) => _commandNames.ContainsKey(item.Name);

        public void CopyTo(Command[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<Command> GetEnumerator() => _commandNames.Values.GetEnumerator();

        public bool Remove(Command item)
        {
            bool anyRemoved = false;

            anyRemoved |= _commandNames.Remove(item.Name);
            anyRemoved |= _commandShortcuts.Remove(item);

            return anyRemoved;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
