using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Transform")]
public class TransformNode : Matrix3X3BaseNode
{
    public override Node CreateCopy()
    {
        return new TransformNode();
    }

    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        return input;
    }
}
