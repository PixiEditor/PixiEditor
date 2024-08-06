using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.CommandContext;

public class ShortcutSourceInfo(KeyCombination combination) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType { get; } = CommandExecutionSourceType.Shortcut;
    
    public KeyCombination Shortcut { get; }

    public static CommandExecutionContext GetContext(object parameter, KeyCombination shortcut) =>
        new(parameter, new ShortcutSourceInfo(shortcut));
}
