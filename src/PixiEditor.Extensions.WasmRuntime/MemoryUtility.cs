using System.Text;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public static class MemoryUtility
{
    public static string GetStringFromWasmMemory(int offset, Memory memory)
    {
        var span = memory.GetSpan<byte>(offset);
        int length = 0;
        while (span[length] != 0)
        {
            length++;
        }

        var buffer = new byte[length];
        span[..length].CopyTo(buffer);
        return Encoding.UTF8.GetString(buffer);
    }
}
