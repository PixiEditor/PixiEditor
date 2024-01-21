using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Wasm;

internal class Interop
{
    internal static extern unsafe void LogMessage(char* message);
}
