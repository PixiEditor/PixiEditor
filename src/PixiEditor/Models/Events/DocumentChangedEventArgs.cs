using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Events;

public class DocumentChangedEventArgs
{
    public DocumentChangedEventArgs(Document newDocument, Document oldDocument)
    {
        NewDocument = newDocument;
        OldDocument = oldDocument;
    }

    public Document OldDocument { get; set; }

    public Document NewDocument { get; set; }
}