using PixiEditor.Models.DataHolders;
using System.Collections;
using System.Windows.Input;

namespace PixiEditor.Models.Commands;

public class CommandGroup : IEnumerable<Command>
{
    private readonly Command[] commands;
    private readonly Command[] visibleCommands;

    public string DisplayName { get; set; }

    public bool HasAssignedShortcuts { get; set; }

    public IEnumerable<Command> Commands => commands;

    public IEnumerable<Command> VisibleCommands => visibleCommands;

    public CommandGroup(string displayName, IEnumerable<Command> commands)
    {
        DisplayName = displayName;
        this.commands = commands.ToArray();
        visibleCommands = commands.Where(x => !string.IsNullOrEmpty(x.DisplayName)).ToArray();

        foreach (var command in commands)
        {
            HasAssignedShortcuts |= command.Shortcut.Key != Key.None;
            command.ShortcutChanged += Command_ShortcutChanged;
        }
    }

    private void Command_ShortcutChanged(Command cmd, ShortcutChangedEventArgs args)
    {
        if ((args.NewShortcut != KeyCombination.None && HasAssignedShortcuts) ||
            (args.NewShortcut == KeyCombination.None && !HasAssignedShortcuts))
        {
            // If a shortcut is already assigned and the new shortcut is not none nothing can change
            // If no shortcut is already assigned and the new shortcut is none nothing can change
            return;
        }

        HasAssignedShortcuts = false;

        foreach (var command in commands)
        {
            HasAssignedShortcuts |= command.Shortcut.Key != Key.None;
        }
    }

    public IEnumerator<Command> GetEnumerator() => Commands.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Commands.GetEnumerator();
}