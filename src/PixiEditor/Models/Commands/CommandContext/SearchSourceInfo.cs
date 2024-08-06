namespace PixiEditor.Models.Commands.CommandContext;

public class SearchSourceInfo(string searchTerm) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType => CommandExecutionSourceType.Search;

    public string SearchTerm { get; set; } = searchTerm;

    public static CommandExecutionContext GetContext(string searchTerm) =>
        new(null, new SearchSourceInfo(searchTerm));
}
