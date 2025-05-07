namespace PixiEditor.Extensions.CommonApi.IO;

public interface IDocumentProvider
{
   public void ImportFile(string path, bool associatePath = true);
}
