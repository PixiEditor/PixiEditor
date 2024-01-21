using System.Runtime.InteropServices;
using PixiEditor.Extensions.Wasm;

namespace SampleExtension.WASM;

public static class Entry
{
    [UnmanagedCallersOnly(EntryPoint = "Entry")]
    public static void EntryPoint()
    {
        SampleExtension extension = new SampleExtension();
        extension.OnLoaded();
    }
}
