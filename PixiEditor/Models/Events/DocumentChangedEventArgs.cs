using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Text;

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
