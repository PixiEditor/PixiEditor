namespace PixiEditor.Extensions.CommonApi.Commands;

public interface ICommandProvider
{
    public void RegisterCommand(CommandMetadata command, Action execute, Func<bool>? canExecute = null);
    public void RegisterCommand(CommandMetadata command, Action<string> execute, Func<string, bool>? canExecute = null);
    public void RegisterCommand(CommandMetadata command, Action<byte[]> execute, Func<byte[], bool>? canExecute = null);
    public void InvokeCommand(string commandName);
    public void InvokeCommand(string commandName, object? parameter);
    public bool CommandExists(string commandName);
}
