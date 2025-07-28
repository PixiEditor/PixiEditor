using PixiEditor.Extensions.CommonApi.Documents;
using PixiEditor.Extensions.Sdk.Api.Documents;
using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static IDocument GetActiveDocument()
    {
        string document = Native.get_active_document();
        if (document == null || !Guid.TryParse(document, out Guid id))
            return null;

        return new Document(id);
    }

    public static IDocument? ImportFile(string path, bool associatePath)
    {
        string document = Native.import_file(path, associatePath);
        if (document == null || !Guid.TryParse(document, out Guid id))
            return null;

        return new Document(id);
    }
    public static IDocument? ImportDocument(byte[] data)
    {
        string document = Native.import_document(InteropUtility.ByteArrayToIntPtr(data), data.Length);
        if (document == null || !Guid.TryParse(document, out Guid id))
            return null;

        return new Document(id);
    }
}
