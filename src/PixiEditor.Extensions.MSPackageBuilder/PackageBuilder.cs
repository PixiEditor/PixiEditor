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
        new ElementToInclude("extension.json", true), new ElementToInclude("AppBundle/*.wasm", true),
        new ElementToInclude("Localization/", false),
        new ElementToInclude("Resources/", false),
    };

    private static readonly string[] FilesToExclude = new[] { "dotnet.wasm", };

    public static void Build(string buildResultDirectory, string targetDirectory, bool encryptedResources)
    {
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

        if (targetDirectory == buildResultDirectory)
        {
            throw new InvalidOperationException("Build result directory and target directory cannot be the same.");
        }
        
        SimplifiedExtensionMetadata metadata =
            JsonConvert.DeserializeObject<SimplifiedExtensionMetadata>(
                File.ReadAllText(Path.Combine(buildResultDirectory, "extension.json")));

        var elementsToInclude = new List<ElementToInclude>(ElementsToInclude);
        if(encryptedResources)
        {
            elementsToInclude.Add(new ElementToInclude("resources.data", true) { TargetDirectory = "Resources" });
            elementsToInclude.RemoveAll(x => x.Path == "Resources/");
        }

        foreach (ElementToInclude element in elementsToInclude)
        {
            if (!string.IsNullOrEmpty(element.TargetDirectory))
            {
                if (!Directory.Exists(Path.Combine(targetTmpDirectory, element.TargetDirectory)))
                {
                    Directory.CreateDirectory(Path.Combine(targetTmpDirectory, element.TargetDirectory));
                }
            }

            string finalDir = string.IsNullOrEmpty(element.TargetDirectory)
                ? targetTmpDirectory
                : Path.Combine(targetTmpDirectory, element.TargetDirectory);

            if (element.Type == ElementToIncludeType.File)
            {
                CopyFile(element.Path, buildResultDirectory, finalDir, element.IsRequired);
            }
            else
            {
                CopyDirectory(element.Path, buildResultDirectory, finalDir, element.IsRequired, metadata);
            }
        }

        string packagePath = Path.Combine(targetDirectory, $"{metadata.UniqueName}.pixiext");
        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }

        ZipFile.CreateFromDirectory(targetTmpDirectory, packagePath);

        Directory.Delete(targetTmpDirectory, true);
    }

    private static void CopyFile(string elementPath, string buildResultDirectory, string targetDirectory,
        bool elementIsRequired)
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

    private static void CopyDirectory(string elementPath, string buildResultDirectory, string targetDirectory,
        bool elementIsRequired, SimplifiedExtensionMetadata metadata)
    {
        string directoryPath = Path.Combine(buildResultDirectory, elementPath);
        if (!Directory.Exists(directoryPath))
        {
            if (elementIsRequired)
            {
                throw new DirectoryNotFoundException(
                    $"Directory {elementPath} was not found in {buildResultDirectory}.");
            }

            return;
        }

        string targetDir = Path.Combine(targetDirectory, elementPath);
        Directory.CreateDirectory(targetDir);
        var files = Directory.GetFiles(directoryPath);
        var directories = Directory.GetDirectories(directoryPath);
        foreach (string file in files)
        {
            string destination = Path.Combine(targetDir, Path.GetFileName(file));
            if(FileIsLocale(elementPath))
            {
                WriteLocale(file, destination, metadata.UniqueName);
                continue;
            }
            
            File.Copy(file, destination, true);
        }
        
        foreach (string directory in directories)
        {
            CopyDirectory(Path.Combine(elementPath, new DirectoryInfo(directory).Name), buildResultDirectory, targetDirectory, elementIsRequired, metadata);
        }
    }

    private static void WriteLocale(string file, string destination, string metadataUniqueName)
    {
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file));
        Dictionary<string, string> newDict = new();
        foreach (KeyValuePair<string, string> pair in dict)
        {
            if (pair.Key.Contains(":"))
            {
                continue;
            }
            
            newDict.Add($"{metadataUniqueName}:{pair.Key}", pair.Value);
        }
        
        File.WriteAllText(destination, JsonConvert.SerializeObject(newDict));
    }
    
    
    private static bool FileIsLocale(string elementPath)
    {
        return elementPath.EndsWith("Localization/");
    }
}

record ElementToInclude
{
    public string Path { get; set; }
    public string? TargetDirectory { get; set; }
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
