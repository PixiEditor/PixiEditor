using System.Linq;
using System.Windows.Input;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Models.Controllers.Shortcuts
{
    public class Shortcut
    {
        public Shortcut(Key shortcutKey, ICommand command, object commandParameter = null, ModifierKeys modifier = ModifierKeys.None)
        {
            ShortcutKey = shortcutKey;
            Modifier = modifier;
            Command = command;
            CommandParameter = commandParameter;
        }

        public Shortcut(Key shortcutKey, ICommand command, string description, object commandParameter = null, ModifierKeys modifier = ModifierKeys.None)
            : this(shortcutKey, command, commandParameter, modifier)
        {
            Description = description;
        }

        public Key ShortcutKey { get; set; }

        public ModifierKeys Modifier { get; set; }

        /// <summary>
        /// Gets all <see cref="ModifierKeys"/> as an array.
        /// </summary>
        public ModifierKeys[] Modifiers { get => Modifier.GetFlags().Except(new ModifierKeys[] { ModifierKeys.None }).ToArray(); }

        public ICommand Command { get; set; }

        public string Description { get; set; }

        public object CommandParameter { get; set; }

        public void Execute()
        {
            if (Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}