using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static string[] GetOwnedContent()
    {
        IntPtr ptr = Native.get_owned_content();
        if (ptr == IntPtr.Zero)
        {
            return [];
        }

        List<string> contentList = new List<string>();
        byte[] arr = InteropUtility.PrefixedIntPtrToByteArray(ptr);
        int length = BitConverter.ToInt32(arr, 0);
        int offset = 4;
        for (int i = 0; i < length; i++)
        {
            int strLength = BitConverter.ToInt32(arr, offset);
            offset += 4;
            string content = System.Text.Encoding.UTF8.GetString(arr, offset, strLength);
            contentList.Add(content);
            offset += strLength;
        }

        return contentList.ToArray();
    }
}
