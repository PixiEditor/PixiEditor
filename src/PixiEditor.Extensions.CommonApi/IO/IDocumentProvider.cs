using PixiEditor.Extensions.CommonApi.Documents;

namespace PixiEditor.Extensions.CommonApi.IO;

public interface IDocumentProvider
{
   public IDocument? ActiveDocument { get; }
   public IDocument? ImportFile(string path, bool associatePath = true);
   public IDocument? ImportDocument(byte[] data);

   public IDocument? GetDocument(Guid id);
}
