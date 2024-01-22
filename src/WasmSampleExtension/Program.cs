using PixiEditor.Extensions.Wasm;

namespace SampleExtension.WASM;

public static class Program
{
    static SampleExtension extension = new SampleExtension();
    public static void Main(string[] args)
    {
        ExtensionEvents.OnInitialized += OnExtensionInitialized;
        extension = new SampleExtension();
        extension.OnLoaded();
    }

    private static void OnExtensionInitialized()
    {
        extension.OnInitialized();
    }
}
