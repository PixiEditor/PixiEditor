using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Wasm;

public static class ExtensionEvents
{
    public static event Action OnInitialized;

    internal static void Initialize()
    {
        OnInitialized?.Invoke();
    }
}
