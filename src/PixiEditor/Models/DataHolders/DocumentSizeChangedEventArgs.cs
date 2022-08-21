using ChunkyImageLib.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DataHolders;

internal class DocumentSizeChangedEventArgs
{
    public DocumentSizeChangedEventArgs(DocumentViewModel document, VecI oldSize, VecI newSize)
    {
        Document = document;
        OldSize = oldSize;
        NewSize = newSize;
    }

    public VecI OldSize { get; }
    public VecI NewSize { get; }
    public DocumentViewModel Document { get; }
}
