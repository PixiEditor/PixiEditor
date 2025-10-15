using Avalonia.Platform;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.BrushEngine;

internal class Brush : IBrush
{
    public IDocument Document { get; set; }
    IReadOnlyDocument IBrush.Document => Document.AccessInternalReadOnlyDocument();
    public string Name { get; set; }
    public Guid Id { get; } = Guid.NewGuid();

    public Brush(Uri uri)
    {
        using var stream = AssetLoader.Open(uri);
        byte[] buffer = new byte[stream.Length];
        stream.ReadExactly(buffer, 0, buffer.Length);
        var doc = Importer.ImportDocument(buffer, null);

        using var graph = doc.ShareGraph();
        BrushOutputNode outputNode =
            graph.TryAccessData().AllNodes.OfType<BrushOutputNode>().FirstOrDefault();
        string name = Path.GetFileNameWithoutExtension(uri.LocalPath);
        if (outputNode != null)
        {
            name = outputNode.BrushName.Value;
        }

        Name = name;
        Document = doc;
    }

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
