using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PixiEditor.Extensions.WasmRuntime.Utilities;

public static class ResourcesUtility
{
    public static string ToResourcesFullPath(Extension extension, string path)
    {
        string resourcesPath = Path.Combine(Path.GetDirectoryName(extension.Location), "Resources");
        string fullPath = path;

        if (path.StartsWith("/") || path.StartsWith("/Resources/"))
        {
            fullPath = Path.Combine(resourcesPath, path[1..]);
        }
        else if (path.StartsWith("Resources/"))
        {
            fullPath = Path.Combine(resourcesPath, path[10..]);
        }

        return fullPath;
    }

    public static byte[] LoadEncryptedResource(WasmExtensionInstance extension, string path)
    {
        string fullPath = ToResourcesFullPath(extension, "Resources/resources.data");
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The resources.data file was not found.", fullPath);
        }

        byte[] data = File.ReadAllBytes(fullPath);
        using var zipArchive = OpenEncryptedArchive(extension.GetEncryptionKey(), extension.GetEncryptionIV(), data, ZipArchiveMode.Read);

        string openPath = path.TrimStart('/').TrimStart("Resources/".ToCharArray());

        ZipArchiveEntry entry = zipArchive.GetEntry(openPath);
        if (entry == null)
        {
            throw new ArgumentException($"Resource '{path}' not found.", nameof(path));
        }

        using Stream entryStream = entry.Open();
        using MemoryStream resultStream = new MemoryStream();
        entryStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    private static ZipArchive OpenEncryptedArchive(byte[] encryptionKey, byte[] iv, byte[] data, ZipArchiveMode mode)
    {
        ZipArchive zipArchive = null;
        try
        {
            Encryptor encryptor = new Encryptor(encryptionKey, iv);
            var decrypted = encryptor.Decrypt(data);
            var memoryStream = new MemoryStream(decrypted);
            zipArchive = new ZipArchive(memoryStream, mode, true);
            return zipArchive;
        }
        catch
        {
            zipArchive?.Dispose();
            throw;
        }
    }

    public static void WriteEncryptedResource(WasmExtensionInstance extension, string path, byte[] bytes)
    {
        string fullPath = ToResourcesFullPath(extension, "Resources/resources.data");

        Encryptor encryptor = new Encryptor(extension.GetEncryptionKey(), extension.GetEncryptionIV());
        byte[] encryptedInput = File.ReadAllBytes(fullPath);
        byte[] decryptedZipData = encryptor.Decrypt(encryptedInput);

        using var zipStream = new MemoryStream(decryptedZipData, writable: true);
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            string openPath = path.TrimStart('/').TrimStart("Resources/".ToCharArray());

            var entry = archive.GetEntry(openPath);
            entry?.Delete();

            entry = archive.CreateEntry(openPath);
            using var entryStream = entry.Open();
            entryStream.Write(bytes, 0, bytes.Length);
        }

        zipStream.Position = 0;
        byte[] finalEncrypted = encryptor.Encrypt(zipStream.ToArray());
        File.WriteAllBytes(fullPath, finalEncrypted);
    }

    public static string[] GetEncryptedFilesAtPath(WasmExtensionInstance extension, string path, string searchPattern)
    {
        string fullPath = ToResourcesFullPath(extension, "Resources/resources.data");
        byte[] data = File.ReadAllBytes(fullPath);
        using var zipArchive = OpenEncryptedArchive(extension.GetEncryptionKey(), extension.GetEncryptionIV(), data, ZipArchiveMode.Read);

        string openPath = path.TrimStart('/').TrimStart("Resources/".ToCharArray());
        string prefix = path.StartsWith("/") ? "/" : "";
        if(path.StartsWith("Resources/"))
        {
            prefix += "Resources/";
        }

        bool MatchesPattern(string fileName, string pattern)
        {
            string regex = "^" + Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";

            return Regex.IsMatch(fileName, regex, RegexOptions.IgnoreCase);
        }

        List<string> files = new List<string>();
        foreach (var entry in zipArchive.Entries)
        {
            if (entry.FullName.StartsWith(openPath) && MatchesPattern(entry.FullName, searchPattern))
            {
                files.Add(Path.Combine(prefix, entry.FullName));
            }
        }

        return files.ToArray();
    }

    public static bool HasEncryptedResource(WasmExtensionInstance extension, string path)
    {
        string fullPath = ToResourcesFullPath(extension, "Resources/resources.data");
        if (!File.Exists(fullPath))
        {
            return false;
        }

        byte[] data = File.ReadAllBytes(fullPath);
        using var zipArchive = OpenEncryptedArchive(extension.GetEncryptionKey(), extension.GetEncryptionIV(), data, ZipArchiveMode.Read);

        string openPath = path.TrimStart('/').TrimStart("Resources/".ToCharArray());
        return zipArchive.GetEntry(openPath) != null;
    }
}

internal class Encryptor
{
    private byte[] key;
    private byte[] iv;

    public Encryptor(byte[] key, byte[] iv)
    {
        this.key = key;
        this.iv = iv;
    }


    public byte[] Encrypt(byte[] data)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;

            aes.Key = key;
            aes.IV = iv;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, encryptor);
            }
        }
    }

    public byte[] Decrypt(byte[] data)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;

            aes.Key = key;
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, decryptor);
            }
        }
    }

    private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using (var ms = new MemoryStream())
        using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
        {
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return ms.ToArray();
        }
    }
}
