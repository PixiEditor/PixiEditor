namespace PixiEditor.Models.Commands.CommandContext;

public class ExtensionSourceInfo : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType { get; } = CommandExecutionSourceType.Extension;


    public ExtensionSourceInfo()
    {
    }
}
