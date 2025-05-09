using PixiEditor.Extensions.CommonApi.IO;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Models.ExtensionServices;

internal class DocumentProvider : IDocumentProvider
{
    private FileViewModel fileViewModel;

    public DocumentProvider(FileViewModel fileViewModel)
    {
        this.fileViewModel = fileViewModel;
    }

    public void ImportFile(string path, bool associatePath = true)
    {
        fileViewModel.OpenFromPath(path, associatePath);
    }
}
