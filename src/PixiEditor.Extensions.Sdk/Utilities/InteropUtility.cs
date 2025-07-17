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

    public static byte[] PrefixedIntPtrToByteArray(IntPtr ptr)
    {
        // Read the first 4 bytes as an integer (length of the data)
        int length = Marshal.ReadInt32(ptr);
        if (length <= 0)
        {
            return [];
        }

        // Read the next bytes based on the length
        byte[] array = new byte[length];
        Marshal.Copy(ptr + sizeof(int), array, 0, length);
        return array;
    }

    public static string[] IntPtrToStringArray(IntPtr ptr)
    {
        List<string> list = new List<string>();
        byte[] arr = PrefixedIntPtrToByteArray(ptr);
        if (arr.Length == 0)
        {
            return [];
        }

        int length = BitConverter.ToInt32(arr, 0);
        int offset = 4;
        for (int i = 0; i < length; i++)
        {
            int strLength = BitConverter.ToInt32(arr, offset);
            offset += 4;
            string content = System.Text.Encoding.UTF8.GetString(arr, offset, strLength);
            list.Add(content);
            offset += strLength;
        }

        return list.ToArray();
    }
}
