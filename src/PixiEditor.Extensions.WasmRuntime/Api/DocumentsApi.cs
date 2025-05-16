using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class DocumentsApi : ApiGroupHandler
{
    [ApiFunction("import_file")]
    public string ImportFile(string path, bool associatePath = false)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.OpenDocuments, "ImportFile");

        string fullPath = ResourcesUtility.ToResourcesFullPath(Extension, path);

        string id = string.Empty;
        if (File.Exists(fullPath))
        {
            id = Api.Documents.ImportFile(fullPath, associatePath)?.Id.ToString() ?? string.Empty;
        }

        return id;
    }

    [ApiFunction("get_active_document")]
    public string GetActiveDocument()
    {
        var activeDocument = Api.Documents.ActiveDocument;
        string id = activeDocument?.Id.ToString() ?? string.Empty;
        return id;
    }

    [ApiFunction("resize_document")]
    public void ResizeDocument(string documentId, int width, int height)
    {
        if (!Guid.TryParse(documentId, out Guid id))
        {
            throw new ArgumentException("Invalid document ID");
        }

        var document = Api.Documents.GetDocument(id);
        if (document == null)
        {
            throw new ArgumentException("Document not found");
        }

        document.Resize(width, height);
    }
}
