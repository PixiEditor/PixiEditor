using System.Text;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public static class MemoryUtility
{
    public static string GetStringFromWasmMemory(int offset, int length, Memory memory)
    {
        //TODO: memory.ReadString is a thing
        var span = memory.GetSpan<byte>(offset, length);
        return Encoding.UTF8.GetString(span);
    }

    public static Span<T> GetSpanFromWasmMemory<T>(int bodyOffset, int bodyLength, Memory memory) where T : unmanaged
    {
        var span = memory.GetSpan<T>(bodyOffset, bodyLength);
        return span;
    }

    public static int WriteInt32(Instance instance, Memory memory, int value)
    {
        // TODO: cache malloc function
        var malloc = instance.GetFunction<int, int>("malloc");

        const int length = 4;
        var ptr = malloc.Invoke(length);
        memory.WriteInt32(ptr, value);
        return ptr;
    }

    public static int WriteString(Instance instance, Memory memory, string value)
    {
        var malloc = instance.GetFunction<int, int>("malloc");

        var length = value.Length;
        var ptr = malloc.Invoke(length);
        memory.WriteString(ptr, value, Encoding.UTF8);
        return ptr;
    }
}
