using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Scale")]
public class ScaleNode : Matrix3X3BaseNode
{
    public InputProperty<VecD> Scale { get; }
    public InputProperty<VecD> Center { get; }

    public ScaleNode()
    {
        Scale = CreateInput("Scale", "SCALE", new VecD(1, 1));
        Center = CreateInput("Center", "CENTER", new VecD(0, 0));
    }

    protected override Matrix3X3 CalculateMatrix(Matrix3X3 input)
    {
        Matrix3X3 scaled = Matrix3X3.CreateScale((float)Scale.Value.X, (float)Scale.Value.Y, (float)Center.Value.X, (float)Center.Value.Y);
        return input.PostConcat(scaled);
    }

    public override Node CreateCopy()
    {
        return new ScaleNode();
    }
}
