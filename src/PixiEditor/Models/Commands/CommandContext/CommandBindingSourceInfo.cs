namespace PixiEditor.Models.Commands.CommandContext;

public class CommandBindingSourceInfo(string tag) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType => CommandExecutionSourceType.CommandBinding;

    public string Tag { get; } = tag;
}
