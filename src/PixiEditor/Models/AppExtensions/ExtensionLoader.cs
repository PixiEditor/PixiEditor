using System.IO;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Models.IO;
using PixiEditor.Platform;

namespace PixiEditor.Models.AppExtensions;

internal class ExtensionLoader
{
    public List<Extension> LoadedExtensions { get; } = new();

    public ExtensionLoader()
    {
        ValidateExtensionFolder();
    }

    public void LoadExtensions()
    {
        var directories = Directory.GetDirectories(Paths.ExtensionsFullPath);
        foreach (var directory in directories)
        {
            string packageJsonPath = Path.Combine(directory, "extension.json");
            bool isExtension = File.Exists(packageJsonPath);
            if (isExtension)
            {
                LoadExtension(packageJsonPath);
            }
        }
    }

    public void InitializeExtensions(ExtensionServices pixiEditorApi)
    {
        foreach (var extension in LoadedExtensions)
        {
            extension.Initialize(pixiEditorApi);
        }
    }

    private void LoadExtension(string packageJsonPath)
    {
        string json = File.ReadAllText(packageJsonPath);
        try
        {
            var metadata = JsonConvert.DeserializeObject<ExtensionMetadata>(json);
            ValidateMetadata(metadata);
            var extension = LoadExtensionEntry(Path.GetDirectoryName(packageJsonPath), metadata);
            extension.Load();
            LoadedExtensions.Add(extension);
        }
        catch (JsonException)
        {
            MessageBox.Show(new LocalizedString("ERROR_INVALID_PACKAGE", packageJsonPath), "ERROR");
        }
        catch (ExtensionException ex)
        {
            MessageBox.Show(ex.DisplayMessage, "ERROR");
        }
        catch (Exception ex)
        {
            MessageBox.Show(new LocalizedString("ERROR_LOADING_PACKAGE", packageJsonPath), "ERROR");
        }
    }

    private void ValidateMetadata(ExtensionMetadata metadata)
    {
        if (string.IsNullOrEmpty(metadata.UniqueName))
        {
            throw new MissingMetadataException("Description");
        }

        if (metadata.UniqueName.StartsWith("pixieditor".Trim(), StringComparison.OrdinalIgnoreCase))
        {
            if(!IsOfficialAssemblyLegit(metadata.UniqueName))
            {
                throw new ForbiddenUniqueNameExtension();
            }
        }
        // TODO: Validate if unique name is unique

        if (string.IsNullOrEmpty(metadata.DisplayName))
        {
            throw new MissingMetadataException("DisplayName");
        }

        if (string.IsNullOrEmpty(metadata.Version))
        {
            throw new MissingMetadataException("Version");
        }
    }

    private bool IsOfficialAssemblyLegit(string metadataUniqueName)
    {
        return true; //TODO: Perform assembly secret number check
    }

    private Extension LoadExtensionEntry(string assemblyFolder, ExtensionMetadata metadata)
    {
        string[] dlls = Directory.GetFiles(assemblyFolder, "*.dll");
        Assembly? entryAssembly = GetEntryAssembly(dlls, out Type extensionType);
        if (entryAssembly is null)
        {
            throw new NoEntryAssemblyException(assemblyFolder);
        }

        var extension = (Extension)Activator.CreateInstance(extensionType);
        if (extension is null)
        {
            throw new NoClassEntryException(entryAssembly.Location);
        }

        extension.ProvideMetadata(metadata);
        return extension;
    }

    private Assembly? GetEntryAssembly(string[] dlls, out Type extensionType)
    {
        foreach (var dll in dlls)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                extensionType = assembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(Extension)));
                if (extensionType is not null)
                {
                    return assembly;
                }
            }
            catch
            {
                // ignored
            }
        }

        extensionType = null;
        return null;
    }

    private void ValidateExtensionFolder()
    {
        if (!Directory.Exists(Paths.ExtensionsFullPath))
        {
            Directory.CreateDirectory(Paths.ExtensionsFullPath);
        }
    }
}
