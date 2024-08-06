namespace PixiEditor.Models.Commands.CommandContext;

public interface ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType { get; }
}
