using System.Runtime.InteropServices;
using System.Text;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime;

public class WasmMemoryUtility
{
    private Instance instance;
    private readonly Memory memory;
    private Func<int, int> malloc;
    private Action<int> free;

    public WasmMemoryUtility(Instance instance)
    {
        this.instance = instance;
        memory = instance.GetMemory("memory");
        malloc = instance.GetFunction<int, int>("malloc");
        free = instance.GetAction<int>("free");
    }

    public string GetString(int offset, int length)
    {
        //TODO: memory.ReadString is a thing
        var span = memory.GetSpan<byte>(offset, length);
        return Encoding.UTF8.GetString(span);
    }

    public int WriteInt32(int value)
    {
        const int length = 4;
        var ptr = malloc.Invoke(length);
        memory.WriteInt32(ptr, value);
        return ptr;
    }

    public int WriteString(string value)
    {
        string valueWithNullTerminator = value + '\0';
        var ptr = malloc.Invoke(valueWithNullTerminator.Length);
        memory.WriteString(ptr, valueWithNullTerminator);
        return ptr;
    }

    public void Free(int address)
    {
        free.Invoke(address);
    }

    public int WriteSpan(byte[] bytes)
    {
        var length = bytes.Length;
        var ptr = malloc.Invoke(length);

        var span = memory.GetSpan<byte>(ptr, length);
        bytes.CopyTo(span);
        return ptr;
    }
}
