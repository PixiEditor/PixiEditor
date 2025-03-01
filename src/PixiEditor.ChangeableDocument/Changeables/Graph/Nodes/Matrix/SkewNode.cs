using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Skew")]
public class SkewNode : Matrix3X3BaseNode
{
    public InputProperty<VecD> Skew { get; }

    public SkewNode()
    {
        Skew = CreateInput("Skew", "SKEW", VecD.Zero);
    }

    protected override Matrix3X3 CalculateMatrix(Matrix3X3 input)
    {
        Matrix3X3 matrix = Matrix3X3.CreateSkew((float)Skew.Value.X, (float)Skew.Value.Y);
        return input.PostConcat(matrix);
    }

    public override Node CreateCopy()
    {
        return new SkewNode();
    }
}
