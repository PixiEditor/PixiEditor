using PixiEditor.Extensions.Commands;
using PixiEditor.Extensions.CommonApi.Commands;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class CommandModule : ApiModule
{
    public ICommandProvider CommandProvider { get; }
    public ICommandSupervisor CommandSupervisor { get; }

    public CommandModule(WasmExtensionInstance extension, ICommandProvider commandProvider, ICommandSupervisor supervisor) : base(extension)
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
}
