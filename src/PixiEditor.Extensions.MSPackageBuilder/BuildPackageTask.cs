using System.Diagnostics;
using Microsoft.Build.Framework;

namespace PixiEditor.Extensions.MSPackageBuilder;

public class BuildPackageTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public string BuildResultDirectory { get; set; } = default!;
    
    [Required]
    public string TargetDirectory { get; set; } = default!;

    public string EncryptionKey { get; set; }

    public override bool Execute()
    {
        try
        {
            PackageBuilder.Build(BuildResultDirectory, TargetDirectory, !string.IsNullOrEmpty(EncryptionKey));
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e, true, true, null);
            return false;
        }

        return true;
    }
}
