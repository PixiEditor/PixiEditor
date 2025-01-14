using System.IO;

namespace PixiEditor.Cake.Builder.PackageBuilders;

public abstract class PackageBuilder
{
    public abstract PackageBuildInfo BuildPackage(BuildContext context);

    protected void CreateDirectoryIfMissing(string path, bool clean = true)
    {
        if (clean && Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    protected void CopyFilesOverwrite(string sourcePath, string destinationPath, bool recursive = true,
        string ignore = "")
    {
        if (sourcePath == ignore)
        {
            return;
        }

        CreateDirectoryIfMissing(destinationPath);
        foreach (string file in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(file);

            if (fileName == ignore)
            {
                continue;
            }

            string destFile = Path.Combine(destinationPath, fileName);
            File.Copy(file, destFile, true);
        }

        if (recursive)
        {
            foreach (string dir in Directory.GetDirectories(sourcePath))
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(destinationPath, dirName);
                CopyFilesOverwrite(dir, destDir, true, ignore);
            }
        }
    }
}

public struct PackageBuildInfo
{
    public bool Success { get; set; }
    public string PathToPackage { get; set; }

    public string? Error { get; set; }

    public PackageBuildInfo(bool success, string pathToPackage)
    {
        Success = success;
        PathToPackage = pathToPackage;
    }

    public static PackageBuildInfo Succeed(string pathToPackage)
    {
        return new PackageBuildInfo(true, pathToPackage);
    }

    public static PackageBuildInfo Fail(string error)
    {
        return new PackageBuildInfo(false, null);
    }
}
