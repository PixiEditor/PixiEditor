using System.IO.Compression;

namespace PixiEditor.Extensions.MSPackageBuilder;

/// <summary>
///   Class that is used to create .pixiext packages. 
/// </summary>
public static class PackageBuilder
{
    private static readonly ElementToInclude[] ElementsToInclude = new[]
    {
        new ElementToInclude("extension.json", true),
        new ElementToInclude("AppBundle/*.wasm", true),
        new ElementToInclude("Localization/", false),
    };
    
    public static void Build(string buildResultDirectory, string targetDirectory, Action<string> log)
    {
        if (!Directory.Exists(buildResultDirectory))
        {
            throw new DirectoryNotFoundException($"Directory {buildResultDirectory} does not exist.");
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        foreach (ElementToInclude element in ElementsToInclude)
        {
            log($"Copying {element.Path}...");
            if (element.Type == ElementToIncludeType.File)
            {
                CopyFile(element.Path, buildResultDirectory, targetDirectory, element.IsRequired);
            }
            else
            {
                CopyDirectory(element.Path, buildResultDirectory, targetDirectory, element.IsRequired);
            }
        } 
        
        log("Copied all elements. Building package...");
        
        string packagePath = Path.Combine(targetDirectory, "package.pixiext");
        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }
        
        ZipFile.CreateFromDirectory(targetDirectory, packagePath);
        
        log($"Package created at {packagePath}.");
        
        Directory.Delete(targetDirectory, true);
    }

    private static void CopyFile(string elementPath, string buildResultDirectory, string targetDirectory, bool elementIsRequired)
    {
        string[] files = Directory.GetFiles(buildResultDirectory, elementPath);
        if (files.Length == 0)
        {
            if (elementIsRequired)
            {
                throw new FileNotFoundException($"File {elementPath} was not found in {buildResultDirectory}.");
            }
            
            return;
        }

        foreach (string file in files)
        {
            File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)), true);
        }
    }
    
    private static void CopyDirectory(string elementPath, string buildResultDirectory, string targetDirectory, bool elementIsRequired)
    {
        string[] directories = Directory.GetDirectories(buildResultDirectory, elementPath, SearchOption.AllDirectories);
        if (directories.Length == 0)
        {
            if (elementIsRequired)
            {
                throw new DirectoryNotFoundException($"Directory {elementPath} was not found in {buildResultDirectory}.");
            }
            
            return;
        }

        foreach (string directory in directories)
        {
            string targetDir = Path.Combine(targetDirectory, Path.GetFileName(directory));
            Directory.CreateDirectory(targetDir);
            foreach (string file in Directory.GetFiles(directory))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }
        }
    }
}

record ElementToInclude
{
    public string Path { get; set; }
    public bool IsRequired { get; set; }
    
    public ElementToIncludeType Type => Path.EndsWith("/") ? ElementToIncludeType.Directory : ElementToIncludeType.File;

    public ElementToInclude(string path, bool isRequired)
    {
        Path = path;
        IsRequired = isRequired;
    }
}

enum ElementToIncludeType
{
    File,
    Directory
}
