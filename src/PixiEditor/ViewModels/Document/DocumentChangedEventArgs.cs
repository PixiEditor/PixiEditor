namespace PixiEditor.ViewModels.Document;

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
