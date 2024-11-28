using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels;

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
