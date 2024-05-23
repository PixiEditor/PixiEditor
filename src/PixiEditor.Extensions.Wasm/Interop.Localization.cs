using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Wasm;

internal static partial class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string translate_key(string key);
}
