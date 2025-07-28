using System.Text.Json;

namespace PixiEditor.Models.Commands.CommandContext;

public class CommandExecutionContext(object parameter, ICommandExecutionSourceInfo sourceInfo)
{
    public object Parameter { get; set; } = parameter;

    public ICommandExecutionSourceInfo SourceInfo { get; set; } = sourceInfo;
}
