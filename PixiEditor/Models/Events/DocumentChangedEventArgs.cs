using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Events
{
    public class DocumentChangedEventArgs
    {
        public DocumentChangedEventArgs(Document newDocument)
        {
            NewDocument = newDocument;
        }

        public Document NewDocument { get; set; }
    }
}