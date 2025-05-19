using PixiEditor.Extensions.CommonApi.Documents;
using PixiEditor.Extensions.CommonApi.IO;
using PixiEditor.Extensions.Sdk.Api.Documents;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.IO;

public class DocumentProvider : IDocumentProvider
{
    public IDocument ActiveDocument => Interop.GetActiveDocument();

    public IDocument? ImportFile(string path, bool associatePath = true)
    {
        return Interop.ImportFile(path, associatePath);
    }

    public IDocument? GetDocument(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid document ID");

        return new Document(id);
    }
}
