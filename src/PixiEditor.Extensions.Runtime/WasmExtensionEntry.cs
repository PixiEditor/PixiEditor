using PixiEditor.Extensions.WasmRuntime;

namespace PixiEditor.Extensions.Runtime;

public class WasmExtensionEntry : ExtensionEntry
{
    public WasmExtensionInstance Instance { get; }

    public WasmExtensionEntry(WasmExtensionInstance instance)
    {
        Instance = instance;
    }

    public override Extension CreateExtension()
    {
        Instance.Instantiate();
        return Instance;
    }
}
