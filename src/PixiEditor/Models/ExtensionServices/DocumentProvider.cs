using Avalonia.Threading;
using PixiEditor.Extensions.CommonApi.Documents;
using PixiEditor.Extensions.CommonApi.IO;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Models.ExtensionServices;

internal class DocumentProvider : IDocumentProvider
{
    public IDocument? ActiveDocument => fileViewModel.Owner.DocumentManagerSubViewModel.ActiveDocument;
    private FileViewModel fileViewModel;

    public DocumentProvider(FileViewModel fileViewModel)
    {
        this.fileViewModel = fileViewModel;
    }

    public IDocument ImportFile(string path, bool associatePath = true)
    {
        return fileViewModel.OpenFromPath(path, associatePath);
    }

    public IDocument? GetDocument(Guid id)
    {
        var document = fileViewModel.Owner.DocumentManagerSubViewModel.Documents.FirstOrDefault(x => x.Id == id);
        return document;
    }
}
