using System.IO.Compression;
using Newtonsoft.Json;

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
    
    private static readonly string[] FilesToExclude = new[]
    {
        "dotnet.wasm",
    };
    
    public static void Build(string buildResultDirectory, string targetDirectory)
    {
        string packageName = Path.GetFileName(buildResultDirectory);
        if (!Directory.Exists(buildResultDirectory))
        {
            throw new DirectoryNotFoundException($"Directory {buildResultDirectory} does not exist.");
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        
        string targetTmpDirectory = Path.Combine(targetDirectory, "tmp");
        if (Directory.Exists(targetTmpDirectory))
        {
            Directory.Delete(targetTmpDirectory, true);
        }
        
        Directory.CreateDirectory(targetTmpDirectory);
        
        if(targetDirectory == buildResultDirectory)
        {
            throw new InvalidOperationException("Build result directory and target directory cannot be the same.");
        }
        
        foreach (ElementToInclude element in ElementsToInclude)
        {
            if (element.Type == ElementToIncludeType.File)
            {
                CopyFile(element.Path, buildResultDirectory, targetTmpDirectory, element.IsRequired);
            }
            else
            {
                CopyDirectory(element.Path, buildResultDirectory, targetTmpDirectory, element.IsRequired);
            }
        } 
        
        SimplifiedExtensionMetadata metadata = JsonConvert.DeserializeObject<SimplifiedExtensionMetadata>(File.ReadAllText(Path.Combine(buildResultDirectory, "extension.json")));
        
        string packagePath = Path.Combine(targetDirectory, $"{metadata.UniqueName}.pixiext");
        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }
        
        ZipFile.CreateFromDirectory(targetTmpDirectory, packagePath);
        
        Directory.Delete(targetTmpDirectory, true);
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
            if (FilesToExclude.Contains(Path.GetFileName(file)))
            {
                continue;
            }
            
            File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)), true);
        }
    }
    
    private static void CopyDirectory(string elementPath, string buildResultDirectory, string targetDirectory, bool elementIsRequired)
    {
        string pattern = elementPath.EndsWith("/") ? elementPath.Substring(0, elementPath.Length - 1) : elementPath;
        string[] directories = Directory.GetDirectories(buildResultDirectory, pattern);
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

class SimplifiedExtensionMetadata
{
    public string UniqueName { get; set; }
}
