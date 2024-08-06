using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.CommandContext;

public class ShortcutSourceInfo(KeyCombination shortcut) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType => CommandExecutionSourceType.Shortcut;

    public KeyCombination Shortcut { get; } = shortcut;

    public static CommandExecutionContext GetContext(KeyCombination shortcut) =>
        new(null, new ShortcutSourceInfo(shortcut));
}
