using PixiEditor.Extensions;
using PixiEditor.Extensions.WasmRuntime;

namespace PixiEditor.AvaloniaUI.Models.AppExtensions;

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
