using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Transform")]
public class TransformNode : Matrix3X3BaseNode
{
    protected override Matrix3X3 CalculateMatrix(Matrix3X3 input)
    {
        return Input.Value;
    }

    public override Node CreateCopy()
    {
        return new TransformNode();
    }
}
