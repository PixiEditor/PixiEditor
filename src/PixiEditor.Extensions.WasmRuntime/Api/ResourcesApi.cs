using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class ResourcesApi : ApiGroupHandler
{
    [ApiFunction("to_resources_full_path")]
    public string ToResourcesFullPath(string path)
    {
        string fullPath = ResourcesUtility.ToResourcesFullPath(Extension, path);
        return fullPath;
    }
}
