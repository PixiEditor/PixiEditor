using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.BrushEngine;

internal class Brush : IBrush
{
    public IDocument Document { get; set; }
    IReadOnlyDocument IBrush.Document => Document.AccessInternalReadOnlyDocument();
    public string Name { get; set; }
    public Guid Id { get; } = Guid.NewGuid();

    public Brush(string name, IDocument brushDocument)
    {
        Name = name;
        Document = brushDocument;
    }

    public Brush(string name, IDocument brushDocument, Guid id)
    {
        Name = name;
        Document = brushDocument;
        Id = id;
    }

    public override string ToString()
    {
        return Name;
    }
}
