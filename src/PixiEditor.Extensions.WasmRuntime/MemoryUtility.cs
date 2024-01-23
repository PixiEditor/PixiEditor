using System.Text;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public static class MemoryUtility
{
    public static string GetStringFromWasmMemory(int offset, int length, Memory memory)
    {
        var span = memory.GetSpan<byte>(offset, length);
        
        return Encoding.UTF8.GetString(span);
    }
}
