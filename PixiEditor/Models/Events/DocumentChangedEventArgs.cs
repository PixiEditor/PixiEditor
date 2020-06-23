using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Events
{
    public class DocumentChangedEventArgs
    {
        public Document NewDocument { get; set; }

        public DocumentChangedEventArgs(Document newDocument)
        {
            NewDocument = newDocument;
        }
    }
}