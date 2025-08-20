using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo("BrushOutput")]
public class BrushOutputNode : Node
{
    public InputProperty<ShapeVectorData> VectorShape { get; }

    public BrushOutputNode()
    {
        VectorShape = CreateInput<ShapeVectorData>("VectorShape", "VECTOR_SHAPE", null);
    }

    protected override void OnExecute(RenderContext context)
    {

    }

    public override Node CreateCopy()
    {
        return new BrushOutputNode();
    }
}
