namespace PixiEditor.Extensions.WasmRuntime.Utilities;

public static class ResourcesUtility
{
    public static string ToResourcesFullPath(Extension extension, string path)
    {
        string resourcesPath = Path.Combine(Path.GetDirectoryName(extension.Location), "Resources");
        string fullPath = path;

        if (path.StartsWith("/") || path.StartsWith("/Resources/"))
        {
            fullPath = Path.Combine(resourcesPath, path[1..]);
        }
        else if (path.StartsWith("Resources/"))
        {
            fullPath = Path.Combine(resourcesPath, path[10..]);
        }

        return fullPath;
    }
}
