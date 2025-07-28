namespace PixiEditor.Models.Commands.CommandContext;

public class MenuSourceInfo(MenuType menuType) : ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType { get; } = CommandExecutionSourceType.Menu;

    public MenuType MenuType { get; } = menuType;
}
