using System.Windows.Input;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Tools;

namespace PixiEditor.Models.Controllers;

internal class ShortcutController
{
    public static bool ShortcutExecutionBlocked => _shortcutExecutionBlockers.Count > 0;

    private static readonly List<string> _shortcutExecutionBlockers = new List<string>();

    public IEnumerable<Command> LastCommands { get; private set; }

    public Dictionary<KeyCombination, ToolViewModel> TransientShortcuts { get; set; } = new();

    public static void BlockShortcutExection(string blocker)
    {
        if (_shortcutExecutionBlockers.Contains(blocker)) return;
        _shortcutExecutionBlockers.Add(blocker);
    }

    public static void UnblockShortcutExecution(string blocker)
    {
        if (!_shortcutExecutionBlockers.Contains(blocker)) return;
        _shortcutExecutionBlockers.Remove(blocker);
    }

    public static void UnblockShortcutExecutionAll()
    {
        _shortcutExecutionBlockers.Clear();
    }

    public KeyCombination GetToolShortcut<T>()
    {
        return GetToolShortcut(typeof(T));
    }

    public KeyCombination GetToolShortcut(Type type)
    {
        return CommandController.Current.Commands.First(x => x is Command.ToolCommand tool && tool.ToolType == type).Shortcut;
    }

    public void KeyPressed(Key key, ModifierKeys modifiers)
    {
        KeyCombination shortcut = new(key, modifiers);

        if (!ShortcutExecutionBlocked)
        {
            var commands = CommandController.Current.Commands[shortcut];

            if (!commands.Any())
            {
                return;
            }

            LastCommands = commands;

            foreach (var command in CommandController.Current.Commands[shortcut])
            {
                command.Execute();
            }
        }
    }
}
