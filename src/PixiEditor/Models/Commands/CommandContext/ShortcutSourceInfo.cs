using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.CommandContext;

public class ShortcutSourceInfo(KeyCombination shortcut, bool isRepeat) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType => CommandExecutionSourceType.Shortcut;

    public KeyCombination Shortcut { get; } = shortcut;

    public bool IsRepeat { get; set; } = isRepeat;

    public static CommandExecutionContext GetContext(KeyCombination shortcut, bool isRepeat) =>
        new(null, new ShortcutSourceInfo(shortcut, isRepeat));
}
