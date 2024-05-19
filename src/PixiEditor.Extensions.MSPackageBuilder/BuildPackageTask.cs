using Microsoft.Build.Framework;

namespace PixiEditor.Extensions.MSPackageBuilder;

public class BuildPackageTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public string BuildResultDirectory { get; set; } = default!;
    
    [Required]
    public string TargetDirectory { get; set; } = default!;
    
    public override bool Execute()
    {
        string absoluteBuildResultDirectory = Path.GetFullPath(BuildResultDirectory);
        string absoluteTargetDirectory = Path.GetFullPath(TargetDirectory);

        try
        {
            PackageBuilder.Build(absoluteBuildResultDirectory, absoluteTargetDirectory,
                (message) => Log.LogMessage(message, MessageImportance.Normal));
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e, true, true, null);
            return false;
        }

        return true;
    }
}
