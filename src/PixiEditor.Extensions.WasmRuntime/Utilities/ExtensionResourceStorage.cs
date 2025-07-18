using PixiEditor.Extensions.IO;

namespace PixiEditor.Extensions.WasmRuntime.Utilities;

public class ExtensionResourceStorage : IResourceStorage
{
    public WasmExtensionInstance Extension { get; }

    public ExtensionResourceStorage(WasmExtensionInstance extension)
    {
        Extension = extension ?? throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
    }


    public Stream GetResourceStream(string resourcePath)
    {
        if (Extension.GetEncryptionKey()?.Length == 0 || Extension.GetEncryptionIV()?.Length == 0)
        {
            return File.OpenRead(ResourcesUtility.ToResourcesFullPath(Extension, resourcePath));
        }

        byte[] data = ResourcesUtility.LoadEncryptedResource(Extension, resourcePath);
        return new MemoryStream(data);
    }

    public bool Exists(string path)
    {
        try
        {
            string fullPath = ResourcesUtility.ToResourcesFullPath(Extension, path);
            if(Extension.HasEncryptedResources)
            {
                return ResourcesUtility.HasEncryptedResource(Extension, path);
            }
            else
            {
                return File.Exists(fullPath);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}
