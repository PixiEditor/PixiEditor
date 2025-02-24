using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Rotate")]
public class RotateNode : Matrix3X3BaseNode
{
    public InputProperty<RotationType> RotationType { get; }
    public InputProperty<double> Angle { get; }
    public InputProperty<VecD> Center { get; }

    public RotateNode()
    {
        RotationType = CreateInput("RotationType", "ROTATION_TYPE", Nodes.Matrix.RotationType.Degrees);
        Angle = CreateInput("Angle", "ANGLE", 0.0);
        Center = CreateInput("Center", "CENTER", new VecD(0, 0));
    }

    protected override Matrix3X3 CalculateMatrix(Matrix3X3 input)
    {
        VecD scaledCenter = new VecD(Center.Value.X, Center.Value.Y);
        Matrix3X3 rotated = RotationType.Value switch
        {
            Nodes.Matrix.RotationType.Degrees => Matrix3X3.CreateRotationDegrees((float)Angle.Value, (float)scaledCenter.X, (float)scaledCenter.Y),
            Nodes.Matrix.RotationType.Radians => Matrix3X3.CreateRotation((float)Angle.Value, (float)scaledCenter.X, (float)scaledCenter.Y),
            _ => throw new ArgumentOutOfRangeException()
        };

        return input.PostConcat(rotated);
    }

    public override Node CreateCopy()
    {
        return new RotateNode();
    }
}

public enum RotationType
{
    Degrees,
    Radians
}
