using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.BrushEngine;

internal class Brush
{
    public IDocument Document { get; set; }
    public string Name { get; set; }

    public Brush(string name, IDocument brushDocument)
    {
        Name = name;
        Document = brushDocument;
    }

    public override string ToString()
    {
        return Name;
    }
}
