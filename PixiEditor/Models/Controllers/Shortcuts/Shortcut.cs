using System.Windows.Input;

namespace PixiEditor.Models.Controllers
{
    public class Shortcut
    {
        public Key ShortcutKey { get; set; }
        public ModifierKeys Modifier { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        public Shortcut(Key shortcutKey, ICommand command, object commandParameter = null, ModifierKeys modifier = ModifierKeys.None)
        {
            ShortcutKey = shortcutKey;
            Modifier = modifier;
            Command = command;
            CommandParameter = commandParameter;
        }

        public void Execute()
        {
            if (Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
