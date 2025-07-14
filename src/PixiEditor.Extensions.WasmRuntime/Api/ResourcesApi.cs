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

    [ApiFunction("load_encrypted_resource")]
    public byte[] LoadEncryptedResource(string path)
    {
        var data = ResourcesUtility.LoadEncryptedResource(Extension, path);
        return data;
    }

    [ApiFunction("write_encrypted_resource")]
    public void WriteEncryptedResource(string path, Span<byte> data)
    {
        ResourcesUtility.WriteEncryptedResource(Extension, path, data.ToArray());
    }

    [ApiFunction("get_encrypted_files_at_path")]
    public byte[] GetEncryptedFilesAtPath(string path, string searchPattern)
    {
        var files = ResourcesUtility.GetEncryptedFilesAtPath(Extension, path, searchPattern);
        byte[] filesArray = InteropUtility.SerializeToBytes(files);
        return filesArray;
    }
}
