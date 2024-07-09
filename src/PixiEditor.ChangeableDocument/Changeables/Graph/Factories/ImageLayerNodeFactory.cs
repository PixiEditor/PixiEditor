using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Factories;

public class ImageLayerNodeFactory : NodeFactory<ImageLayerNode>
{
    public override T CreateNode<T>(IReadOnlyDocument document)
    {
        return (T)(object)new ImageLayerNode(document.Size);
    }
}
