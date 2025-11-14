using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class DocumentGraphPipe : DocumentMemoryPipe<IReadOnlyNodeGraph>
{
    public DocumentGraphPipe(Document document) : base(document)
    {
    }

    protected override IReadOnlyNodeGraph? GetData()
    {
        return Document.NodeGraph;
    }
}
