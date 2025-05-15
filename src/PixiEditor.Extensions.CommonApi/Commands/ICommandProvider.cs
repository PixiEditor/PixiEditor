namespace PixiEditor.Extensions.CommonApi.Commands;

public interface ICommandProvider
{
    public void RegisterCommand(CommandMetadata command, Action execute, Func<bool>? canExecute = null);
    public void InvokeCommand(string commandName);
    public bool CommandExists(string commandName);
}
