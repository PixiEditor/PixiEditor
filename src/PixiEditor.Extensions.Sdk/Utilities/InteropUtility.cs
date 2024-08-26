using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Sdk.Utilities;

public static class InteropUtility
{
    public static IntPtr ByteArrayToIntPtr(byte[] array)
    {
        var ptr = Marshal.AllocHGlobal(array.Length);
        Marshal.Copy(array, 0, ptr, array.Length);
        return ptr;
    }

    public static byte[] IntPtrToByteArray(IntPtr ptr, int length)
    {
        byte[] array = new byte[length];
        Marshal.Copy(ptr, array, 0, length);
        return array;
    }
}
