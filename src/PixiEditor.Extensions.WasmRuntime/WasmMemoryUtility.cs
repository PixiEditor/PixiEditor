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
        var span = memory.GetSpan<byte>(offset, length);
        return Encoding.UTF8.GetString(span);
    }

    public byte[] GetBytes(int offset, int length)
    {
        var span = memory.GetSpan<byte>(offset, length);
        return span.ToArray();
    }

    public Span<byte> GetSpan(int offset, int length)
    {
        return memory.GetSpan<byte>(offset, length);
    }

    public int GetInt32(int offset)
    {
        return memory.ReadInt32(offset);
    }
    
    public int Write<T>(T arg) where T : unmanaged
    {
        var length = Marshal.SizeOf<T>();
        var ptr = malloc.Invoke(length);
        memory.Write(ptr, arg);
        return ptr;
    }

    public int WriteSpan(Span<byte> span)
    {
        return WriteBytes(span.ToArray());
    }

    public int WriteBytes(byte[] bytes)
    {
        var length = bytes.Length;
        var ptr = malloc.Invoke(length);

        var span = memory.GetSpan<byte>(ptr, length);
        bytes.CopyTo(span);
        return ptr;
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
    
    public int WriteDouble(double value)
    {
        const int length = 8;
        var ptr = malloc.Invoke(length);
        memory.WriteDouble(ptr, value);
        return ptr;
    }
    
    public double GetDouble(int offset)
    {
        return memory.ReadDouble(offset);
    }
    
    public float GetSingle(int offset)
    {
        return memory.ReadSingle(offset);
    }
    
    public int WriteBoolean(bool value)
    {
        const int length = 1;
        var ptr = malloc.Invoke(length);
        memory.Write(ptr, value);
        return ptr;
    }
    
    public bool GetBoolean(int offset)
    {
        return memory.Read<bool>(offset);
    }

    public bool ConvertBoolean(int rawValue)
    {
        return Convert.ToBoolean(rawValue);
    }

    public int ConvertBoolean(bool rawValue)
    {
        return Convert.ToInt32(rawValue);
    }

    public void Free(int address)
    {
        free.Invoke(address);
    }
}
