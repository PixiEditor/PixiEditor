using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Build.Framework;

namespace PixiEditor.Api.CGlueMSBuild;

public class EncryptResourcesTask : Microsoft.Build.Utilities.Task
{
    [Required] public string ResourcesPath { get; set; }

    [Required] public string IntermediateOutputPath { get; set; } = string.Empty;

    [Required] public string OutputPath { get; set; } = string.Empty;

    [Output] public string EncryptionKey { get; set; } = string.Empty;

    [Output] public string EncryptionIv { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            if (!Directory.Exists(ResourcesPath))
            {
                Log.LogError($"Resources directory does not exist: {ResourcesPath}");
                return false;
            }

            string[] files = Directory.GetFiles(ResourcesPath, "*.*", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                return true;
            }

            string path = Path.Combine(IntermediateOutputPath, "resources.zip");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            ZipFile.CreateFromDirectory(ResourcesPath, path,
                CompressionLevel.Fastest, false);
            byte[] data = File.ReadAllBytes(Path.Combine(IntermediateOutputPath, "resources.zip"));
            byte[] encryptionKey = new byte[128 / 8];
            byte[] iv = new byte[128 / 8];
            if (EncryptionKey == string.Empty)
            {
                RandomNumberGenerator.Create().GetBytes(encryptionKey);
                EncryptionKey = Convert.ToBase64String(encryptionKey);
            }

            if (EncryptionIv == string.Empty)
            {
                RandomNumberGenerator.Create().GetBytes(iv);
                EncryptionIv = Convert.ToBase64String(iv);
            }

            byte[] encryptedData = Encrypt(data, encryptionKey, iv);
            File.WriteAllBytes(Path.Combine(OutputPath, "resources.data"), encryptedData);

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }

    public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
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
