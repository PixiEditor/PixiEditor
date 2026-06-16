using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("ExpandStroke")]
public class ExpandStrokeNode : ShapeNode<PathVectorData>
{
    public InputProperty<ShapeVectorData> InputShape { get; }

    public ExpandStrokeNode()
    {
        InputShape = CreateInput<ShapeVectorData>("InputShape", "INPUT_SHAPE", null);
    }

    protected override PathVectorData? GetShapeData(RenderContext context)
    {
        if (InputShape.Value is null)
            return null;

        return InputShape.Value.ExpandStroke();
    }

    public override Node CreateCopy()
    {
        return new ExpandStrokeNode();
    }
}
