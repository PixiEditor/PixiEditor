namespace PixiEditor.Models.Commands.CommandContext;

public class SearchSourceInfo() : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType => CommandExecutionSourceType.Search;

    public static CommandExecutionContext GetContext() =>
        new(null, new SearchSourceInfo());
}
