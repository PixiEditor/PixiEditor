using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;

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
