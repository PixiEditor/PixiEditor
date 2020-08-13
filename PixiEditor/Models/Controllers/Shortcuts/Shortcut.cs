using Avalonia.Input;
using ReactiveUI;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers.Shortcuts
{
    public class Shortcut
    {
        public Key ShortcutKey { get; set; }
        public KeyModifiers Modifier { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        public Shortcut(Key shortcutKey, ICommand command, object commandParameter = null,
            KeyModifiers modifier = KeyModifiers.None)
        {
            ShortcutKey = shortcutKey;
            Modifier = modifier;
            Command = command;
            CommandParameter = commandParameter;
        }

        public void Execute()
        {            
            if (Command.CanExecute(CommandParameter)) Command.Execute(CommandParameter);
        }
    }
}