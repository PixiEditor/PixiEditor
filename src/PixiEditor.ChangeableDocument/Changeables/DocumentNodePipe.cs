using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class DocumentNodePipe<T> : DocumentMemoryPipe<T> where T : class, IReadOnlyNode
{
    public Guid NodeGuid { get; }
    public DocumentNodePipe(Document document, Guid nodeGuid) : base(document)
    {
        NodeGuid = nodeGuid;
    }

    protected override T? GetData()
    {
        var foundNode = Document.FindNode(NodeGuid);
        if (foundNode is T casted) return casted;

        return null;
    }
}
