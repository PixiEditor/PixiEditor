using System.Windows.Input;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Commands.Templates;

public class ShortcutCollection : OneToManyDictionary<KeyCombination, string>
{
    public ShortcutCollection() {}
    
    public ShortcutCollection(IEnumerable<KeyValuePair<KeyCombination, IEnumerable<string>>> enumerable) : base(enumerable) 
    { }
    
    public void Add(string commandName, Key key, ModifierKeys modifiers)
    {
        Add(new(key, modifiers), commandName);
    }
    
    /// <summary>
    /// Unassigns a shortcut.
    /// </summary>
    public void Add(string commandName)
    {
        Add(KeyCombination.None, commandName);
    }
}