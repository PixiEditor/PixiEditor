namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

public class ApiModule(WasmExtensionInstance extension)
{
    public WasmExtensionInstance Extension { get; } = extension;
}
