using PixiEditor.Extensions.Commands;
using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class CommandModule : ApiModule
{
    public ICommandProvider CommandProvider { get; }
    public ICommandSupervisor CommandSupervisor { get; }

    public CommandModule(WasmExtensionInstance extension, ICommandProvider commandProvider,
        ICommandSupervisor supervisor) : base(extension)
    {
        CommandProvider = commandProvider;
        CommandSupervisor = supervisor;
    }


    internal void InvokeCommandInvoked(string uniqueName)
    {
        var action = Extension.Instance.GetAction<int>("command_invoked");

        var pathPtr = Extension.WasmMemoryUtility.WriteString(uniqueName);
        action?.Invoke(pathPtr);
    }

    public void InvokeCommandGeneric(string commandName, object? parameter)
    {
        string prefixedName = PrefixedNameUtility.ToCommandUniqueName(Extension.Metadata.UniqueName, commandName, true);

        if (!CommandProvider.CommandExists(prefixedName))
        {
            return;
        }

        if (CommandSupervisor.ValidateCommandPermissions(prefixedName, Extension))
        {
            CommandProvider.InvokeCommand(commandName, parameter);
        }
        else
        {
            Extension.Api.Logger.LogError($"Command {prefixedName} is not accessible from {Extension.Metadata.UniqueName} extension.");
        }
    }
}
