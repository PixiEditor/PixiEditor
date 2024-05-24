using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.Wasm.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string translate_key(string key);
}
