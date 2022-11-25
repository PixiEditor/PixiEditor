using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.Events;

internal class DocumentChangedEventArgs
{
    public DocumentChangedEventArgs(DocumentViewModel newDocument, DocumentViewModel oldDocument)
    {
        NewDocument = newDocument;
        OldDocument = oldDocument;
    }

    public DocumentViewModel OldDocument { get; set; }

    public DocumentViewModel NewDocument { get; set; }
}
