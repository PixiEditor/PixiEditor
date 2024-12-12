using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Factories;

public class ImageLayerNodeFactory : NodeFactory<ImageLayerNode>
{
    public override ImageLayerNode CreateNode(IReadOnlyDocument document)
    {
        return new ImageLayerNode(document.Size, document.ProcessingColorSpace);
    }
}
