namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class ResourcesApi : ApiGroupHandler
{
    [ApiFunction("to_resources_full_path")]
    public string ToResourcesFullPath(string path)
    {
        string resourcesPath = Path.Combine(Path.GetDirectoryName(Extension.Location), "Resources");
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
