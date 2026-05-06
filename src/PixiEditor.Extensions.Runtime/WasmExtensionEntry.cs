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
        try
        {
            Instance.Instantiate();
            return Instance;
        }
        catch (Exception e)
        {
            // if instantiation fails, we want to catch the exception and log it, but not crash the entire application
            Console.WriteLine($"Failed to instantiate extension from {Instance.Location}: {e}");
            return null;
        }
    }
}
