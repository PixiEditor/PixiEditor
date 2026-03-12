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
        return new MemoryStream(ResourcesUtility.LoadResource(Extension, resourcePath));
    }

    public bool Exists(string path)
    {
        try
        {
            return ResourcesUtility.ResourceExists(Extension, path);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
