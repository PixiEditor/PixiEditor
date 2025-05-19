using PixiEditor.Extensions.CommonApi.Documents;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Documents;

public class Document : IDocument
{
    public Guid Id => documentId;
    private Guid documentId;

    internal Document(Guid documentId)
    {
        this.documentId = documentId;
    }


    public void Resize(int width, int height)
    {
        Native.resize_document(documentId.ToString(), width, height);
    }
}
