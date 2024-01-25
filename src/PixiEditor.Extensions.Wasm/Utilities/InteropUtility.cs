using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Wasm.Utilities;

public static class InteropUtility
{
    public static IntPtr ByteArrayToIntPtr(byte[] array)
    {
        var ptr = Marshal.AllocHGlobal(array.Length);
        Marshal.Copy(array, 0, ptr, array.Length);
        return ptr;
    }
}
