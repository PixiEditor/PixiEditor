using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Commands;

public class CommandProvider : ICommandProvider
{
    private Dictionary<string, (Action execute, Func<bool> canExecute)> _commands = new();

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

        _commands.Add(command.UniqueName, (execute, canExecute));

        Interop.RegisterCommand(command);
    }

    public void InvokeCommand(string commandName)
    {
        Native.invoke_command(commandName);
    }

    public bool CommandExists(string commandName)
    {
        return Native.command_exists(commandName);
    }

    private void OnCommandInvoked(string uniqueName)
    {
        if (_commands.TryGetValue(uniqueName, out var command))
        {
            if (command.canExecute == null || command.canExecute())
            {
                command.execute();
            }
        }
    }
}
