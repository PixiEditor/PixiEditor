using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace EncryptTools;

public static class PackageEncryptor
{
    public static bool EncryptResources(string resourcesPath, string intermediateOutputPath, string outputPath,
        ref string keyBase64, ref string ivBase64)
    {
        string[] files = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            return true;
        }

        var path = Path.Combine(intermediateOutputPath, "resources.zip");

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        ZipFile.CreateFromDirectory(resourcesPath, path,
            CompressionLevel.Fastest, false);
        var data = File.ReadAllBytes(Path.Combine(intermediateOutputPath, "resources.zip"));
        var encryptionKey = new byte[128 / 8];
        var iv = new byte[128 / 8];
        if (keyBase64 == string.Empty)
        {
            RandomNumberGenerator.Create().GetBytes(encryptionKey);
            keyBase64 = Convert.ToBase64String(encryptionKey);
        }
        else
        {
            encryptionKey = Convert.FromBase64String(keyBase64);
        }

        if (ivBase64 == string.Empty)
        {
            RandomNumberGenerator.Create().GetBytes(iv);
            ivBase64 = Convert.ToBase64String(iv);
        }
        else
        {
            iv = Convert.FromBase64String(ivBase64);
        }

        var encryptedData = Encrypt(data, encryptionKey, iv);
        File.WriteAllBytes(Path.Combine(outputPath, "resources.data"), encryptedData);

        return true;
    }

    public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
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

    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
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
