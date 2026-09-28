using EncryptTools;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace PixiEditor.Api.CGlueMSBuild;

public class EncryptResourcesTask : Task
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
            var encryptionKey = "";
            var baseIv = "";
            if (PackageEncryptor.EncryptResources(ResourcesPath, IntermediateOutputPath, OutputPath, ref encryptionKey,
                    ref baseIv))
            {
                EncryptionKey = encryptionKey;
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
