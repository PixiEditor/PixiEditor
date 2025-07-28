using System.IO.Compression;
using System.Security.Cryptography;
using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static byte[] LoadResource(string path)
    {
        byte[] encryptionKey = InteropUtility.IntPtrToByteArray(Native.get_encryption_key(), 16);
        byte[] iv = InteropUtility.IntPtrToByteArray(Native.get_encryption_iv(), 16);

        if (encryptionKey.Length == 0 || encryptionKey.All(x => x == 0))
        {
            return File.ReadAllBytes(path);
        }

        return InteropUtility.PrefixedIntPtrToByteArray(Native.load_encrypted_resource(path));
    }


    public static void WriteResource(byte[] data, string path)
    {
        byte[] encryptionKey = InteropUtility.IntPtrToByteArray(Native.get_encryption_key(), 16);
        byte[] iv = InteropUtility.IntPtrToByteArray(Native.get_encryption_iv(), 16);

        if (encryptionKey.Length == 0 || encryptionKey.All(x => x == 0))
        {
            File.WriteAllBytes(path, data);
        }
        else
        {
            Native.write_encrypted_resource(path, InteropUtility.ByteArrayToIntPtr(data), data.Length);
        }
    }

    public static string[] GetFilesAtPath(string path, string searchPattern)
    {
        byte[] encryptionKey = InteropUtility.IntPtrToByteArray(Native.get_encryption_key(), 16);
        byte[] iv = InteropUtility.IntPtrToByteArray(Native.get_encryption_iv(), 16);

        if (encryptionKey.Length == 0 || encryptionKey.All(x => x == 0))
        {
            return Directory.GetFiles(path, searchPattern);
        }

        IntPtr filesPtr = Native.get_encrypted_files_at_path(path, searchPattern);

        if (filesPtr == IntPtr.Zero)
        {
            throw new ArgumentException($"Path '{path}' not found.", nameof(path));
        }

        string[] strArr = InteropUtility.IntPtrToStringArray(filesPtr);
        if (strArr == null || strArr.Length == 0)
        {
            return [];
        }

        return strArr;
    }
}
