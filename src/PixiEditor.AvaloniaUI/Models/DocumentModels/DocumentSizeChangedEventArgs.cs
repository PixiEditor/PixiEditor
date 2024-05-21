using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;

internal class DocumentSizeChangedEventArgs
{
    public DocumentSizeChangedEventArgs(IDocument document, VecI oldSize, VecI newSize)
    {
        Document = document;
        OldSize = oldSize;
        NewSize = newSize;
    }

    public VecI OldSize { get; }
    public VecI NewSize { get; }
    public IDocument Document { get; }
}
