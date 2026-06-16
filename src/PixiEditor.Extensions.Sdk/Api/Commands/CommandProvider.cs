using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Commands;

public class CommandProvider : ICommandProvider
{
    private Dictionary<string, (Action<object> execute, Func<object, bool> canExecute)> _commands = new();

    public CommandProvider()
    {
        Interop.CommandInvoked += OnCommandInvoked;
    }

    public void RegisterCommand(CommandMetadata command, Action execute, Func<bool> canExecute = null)
    {
        if (string.IsNullOrEmpty(command.UniqueName))
        {
            throw new ArgumentException("Command unique name cannot be null or empty.");
        }

        if (_commands.ContainsKey(command.UniqueName))
            throw new ArgumentException($"Command with unique name {command.UniqueName} is already registered.");

        _commands.Add(command.UniqueName, (_ => execute(), _ => canExecute?.Invoke() ?? true));

        Interop.RegisterCommand(command);
    }

    public void RegisterCommand(CommandMetadata command, Action<string> execute, Func<string, bool> canExecute = null)
    {
        if (string.IsNullOrEmpty(command.UniqueName))
        {
            throw new ArgumentException("Command unique name cannot be null or empty.");
        }

        if (_commands.ContainsKey(command.UniqueName))
            throw new ArgumentException($"Command with unique name {command.UniqueName} is already registered.");

        _commands.Add(command.UniqueName, ((param) => execute(param as string), param => canExecute?.Invoke(param as string) ?? true));

        Interop.RegisterCommandStrParam(command);
    }

    public void RegisterCommand(CommandMetadata command, Action<byte[]> execute, Func<byte[], bool> canExecute = null)
    {
        if (string.IsNullOrEmpty(command.UniqueName))
        {
            throw new ArgumentException("Command unique name cannot be null or empty.");
        }

        if (_commands.ContainsKey(command.UniqueName))
            throw new ArgumentException($"Command with unique name {command.UniqueName} is already registered.");

        _commands.Add(command.UniqueName, ((param) => execute(param as byte[]), param => canExecute?.Invoke(param as byte[]) ?? true));

        Interop.RegisterCommandBytesParam(command);
    }

    public void InvokeCommand(string commandName)
    {
        Native.invoke_command(commandName);
    }

    public void InvokeCommand(string commandName, object parameter)
    {
        Interop.InvokeCommandGeneric(commandName, parameter);
    }

    public bool CommandExists(string commandName)
    {
        return Native.command_exists(commandName);
    }

    private void OnCommandInvoked(string uniqueName, object param)
    {
        if (_commands.TryGetValue(uniqueName, out var command))
        {
            if (command.canExecute == null || command.canExecute(param))
            {
                command.execute(param);
            }
        }
    }
}
