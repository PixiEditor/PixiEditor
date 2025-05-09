using PixiEditor.Extensions.CommonApi.Menu;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class CommandModule : ApiModule
{
    public ICommandProvider CommandProvider { get; }

    public CommandModule(WasmExtensionInstance extension, ICommandProvider commandProvider) : base(extension)
    {
        CommandProvider = commandProvider;
    }

    internal void InvokeCommandInvoked(string uniqueName)
    {
        var action = Extension.Instance.GetAction<int>("command_invoked");

        var pathPtr = Extension.WasmMemoryUtility.WriteString(uniqueName);
        action?.Invoke(pathPtr);
    }
}
