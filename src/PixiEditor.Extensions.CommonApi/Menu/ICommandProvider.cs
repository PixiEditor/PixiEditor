using PixiEditor.Extensions.CommonApi.Commands;

namespace PixiEditor.Extensions.CommonApi.Menu;

public interface ICommandProvider
{
    public void RegisterCommand(CommandMetadata command, Action execute, Func<bool>? canExecute = null);
}
