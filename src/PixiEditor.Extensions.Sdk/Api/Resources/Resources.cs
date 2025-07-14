using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Resources;

public static class Resources
{
    public static byte[] ReadAllBytes(string path)
    {
        var bytes = Interop.LoadResource(path);
        if (bytes == null)
        {
            throw new ArgumentException($"Resource '{path}' not found.", nameof(path));
        }

        return bytes;
    }

    public static string ReadAllText(string path)
    {
        var bytes = Interop.LoadResource(path);
        if (bytes == null)
        {
            throw new ArgumentException($"Resource '{path}' not found.", nameof(path));
        }

        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static void WriteAllText(string path, string text)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
        Interop.WriteResource(data, path);
    }

    public static void WriteAllBytes(string path, byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data cannot be null.");
        }

        Interop.WriteResource(data, path);
    }

    public static string[] GetFilesAtPath(string path, string searchPattern = "*")
    {
        var files = Interop.GetFilesAtPath(path, searchPattern);
        if (files == null)
        {
            throw new ArgumentException($"Path '{path}' not found.", nameof(path));
        }

        return files;
    }
}
