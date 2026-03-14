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
        if (!Directory.Exists(ResourcesPath))
        {
            Log.LogError($"Resources directory does not exist: {ResourcesPath}");
            return false;
        }

        try
        {
            string encryptionKey = "";
            string baseIv = "";
            if (PackageEncryptor.EncryptResources(ResourcesPath, IntermediateOutputPath, OutputPath, ref encryptionKey, ref baseIv))
            {
                EncryptionKey  = baseIv;
                EncryptionIv = baseIv;

                return true;
            }
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }

        return false;
    }
}
