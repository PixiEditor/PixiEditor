namespace PixiEditor.Models.Commands.CommandContext;

public class SearchSourceInfo(string searchTerm, int index) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType { get; } = CommandExecutionSourceType.Search;

    public string SearchTerm { get; set; } = searchTerm;

    public int Index { get; set; } = index;

    public static CommandExecutionContext GetContext(object parameter, string searchTerm, int index) =>
        new(parameter, new SearchSourceInfo(searchTerm, index));
}
