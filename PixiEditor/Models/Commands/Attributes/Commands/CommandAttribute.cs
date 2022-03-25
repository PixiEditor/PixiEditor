using PixiEditor.Models.DataHolders;
using System.Windows.Input;

namespace PixiEditor.Models.Commands.Attributes;

public partial class Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class CommandAttribute : Attribute
    {
        public string Name { get; }

        public string Display { get; }

        public string Description { get; }

        public string CanExecute { get; set; }

        public Key Key { get; set; }

        public ModifierKeys Modifiers { get; set; }

        protected CommandAttribute(string name, string display, string description)
        {
            Name = name;
            Display = display;
            Description = description;
        }

        public KeyCombination GetShortcut() => new() { Key = Key, Modifiers = Modifiers };
    }
}
