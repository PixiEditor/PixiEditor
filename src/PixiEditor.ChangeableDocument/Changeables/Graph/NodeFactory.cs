using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public abstract class NodeFactory
{
    public Type NodeType { get; }

    public NodeFactory(Type nodeType)
    {
        NodeType = nodeType;
    }

    public abstract Node CreateNode(IReadOnlyDocument document);
}

public abstract class NodeFactory<T> : NodeFactory where T : Node
{
    public NodeFactory() : base(typeof(T))
    {
    }

    public abstract T CreateNode<T>(IReadOnlyDocument document) where T : Node;

    public override Node CreateNode(IReadOnlyDocument document)
    {
        return CreateNode<T>(document);
    }
}
