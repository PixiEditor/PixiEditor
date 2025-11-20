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
    public string? FilePath { get; }
    public Guid OutputNodeId { get; }
    public Guid PersistentId { get; }
    public string[] EmbeddedTags { get; set; } = Array.Empty<string>();
    public bool IsDuplicable { get; set; } = true;
    public bool IsReadOnly { get; set; } = false;

    public Brush(Uri uri)
    {
        Stream stream;
        if (uri.IsFile)
        {
            stream = File.OpenRead(uri.LocalPath);
            FilePath = uri.LocalPath;
        }
        else
        {
            stream = AssetLoader.Open(uri);
        }

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
            OutputNodeId = outputNode.Id;
            PersistentId = outputNode.PersistentId;
        }
        else
        {
            OutputNodeId = Guid.NewGuid();
            PersistentId = Guid.NewGuid();
        }

        Name = name;
        Document = doc;

        EmbeddedTags = ExtractTags(outputNode)?.ToArray() ?? [];

        stream.Close();
        stream.Dispose();
    }

    public Brush(string name, IDocument brushDocument, string? filePath = null)
    {
        Name = name;
        Document = brushDocument;
        FilePath = filePath ?? brushDocument.FullFilePath;
        BrushOutputNode? outputNode =
            brushDocument.AccessInternalReadOnlyDocument().NodeGraph.AllNodes.OfType<BrushOutputNode>()
                .FirstOrDefault();
        if (outputNode != null)
        {
            OutputNodeId = outputNode.Id;
            PersistentId = outputNode.PersistentId;
            EmbeddedTags = ExtractTags(outputNode)?.ToArray() ?? [];
        }
        else
        {
            OutputNodeId = Guid.NewGuid();
            PersistentId = Guid.NewGuid();
        }
    }

    private static IEnumerable<string> ExtractTags(BrushOutputNode outputNode)
    {
        return outputNode?.Tags.Value
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t => t.Trim());
    }

    public override string ToString()
    {
        return Name;
    }
}
