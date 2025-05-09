using PixiEditor.Extensions.CommonApi.IO;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.IO;

public class DocumentProvider : IDocumentProvider
{
    public void ImportFile(string path, bool associatePath = true)
    {
        Native.import_file(path, associatePath);
    }
}
