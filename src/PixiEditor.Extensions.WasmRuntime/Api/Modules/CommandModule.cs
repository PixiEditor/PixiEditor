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

    internal void InvokeCommandInvoked(string uniqueName, string parameter)
    {
        var action = Extension.Instance.GetAction<int, int>("command_invoked_str_param");

        var pathPtr = Extension.WasmMemoryUtility.WriteString(uniqueName);
        var paramPtr = Extension.WasmMemoryUtility.WriteString(parameter);
        action?.Invoke(pathPtr, paramPtr);
    }

    internal void InvokeCommandInvoked(string uniqueName, Span<byte> parameter)
    {
        var action = Extension.Instance.GetAction<int, int>("command_invoked_generic_param");

        var pathPtr = Extension.WasmMemoryUtility.WriteString(uniqueName);
        var paramPtr = Extension.WasmMemoryUtility.WriteSpan(parameter);
        action?.Invoke(pathPtr, paramPtr);
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
            CommandProvider.InvokeCommand(prefixedName, parameter);
        }
        else
        {
            Extension.Api.Logger.LogError($"Command {prefixedName} is not accessible from {Extension.Metadata.UniqueName} extension.");
        }
    }
}
