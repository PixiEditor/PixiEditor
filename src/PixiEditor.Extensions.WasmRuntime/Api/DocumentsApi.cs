using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class DocumentsApi : ApiGroupHandler
{
    [ApiFunction("import_file")]
    public void ImportFile(string path, bool associatePath = false)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.OpenDocuments, "ImportFile");

        string fullPath = ResourcesUtility.ToResourcesFullPath(Extension, path);

        if (!File.Exists(fullPath))
        {
            return;
        }

        Api.Documents.ImportFile(fullPath, associatePath);
    }
}
