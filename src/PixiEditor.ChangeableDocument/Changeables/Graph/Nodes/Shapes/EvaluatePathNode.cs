using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("EvaluatePath")]
public class EvaluatePathNode : Node
{
    public InputProperty<ShapeVectorData> Shape { get; }
    public InputProperty<double> Offset { get; }
    public InputProperty<bool> NormalizeOffset { get; }

    public OutputProperty<VecD> Position { get; }
    public OutputProperty<VecD> Tangent { get; }
    public OutputProperty<Matrix3X3> Matrix { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.All;

    public EvaluatePathNode()
    {
        Shape = CreateInput<ShapeVectorData>("Shape", "SHAPE", null);
        Offset = CreateInput<double>("NormalizedOffset", "OFFSET", 0.0);
        NormalizeOffset = CreateInput<bool>("NormalizeOffset", "NORMALIZE_OFFSET", true);

        Position = CreateOutput<VecD>("Position", "POSITION", VecD.Zero);
        Tangent = CreateOutput<VecD>("Tangent", "TANGENT", VecD.Zero);
        Matrix = CreateOutput<Matrix3X3>("Matrix", "MATRIX", Matrix3X3.Identity);
    }

    protected override void OnExecute(RenderContext context)
    {
        if(Shape.Value == null)
        {
            Position.Value = VecD.Zero;
            Tangent.Value = VecD.Zero;
            Matrix.Value = Matrix3X3.Identity;
            return;
        }

        double offset = Offset.Value;

        var path = Shape.Value.ToPath(true);

        if (path == null)
        {
            Position.Value = VecD.Zero;
            Tangent.Value = VecD.Zero;
            Matrix.Value = Matrix3X3.Identity;
            return;
        }

        float absoluteOffset = (float)offset;

        if (NormalizeOffset.Value)
        {
            offset = Math.Clamp(offset, 0.0, 1.0);
            double length = path.Length;
            absoluteOffset = (float)(length * offset);
        }

        Vec4D data = path.GetPositionAndTangentAtDistance(absoluteOffset, false);
        Matrix3X3 matrix = path.GetMatrixAtDistance(absoluteOffset, false, PathMeasureMatrixMode.GetPositionAndTangent);

        Position.Value = new VecD(data.X, data.Y);
        Tangent.Value = new VecD(data.Z, data.W);

        Matrix.Value = matrix;
    }

    public override Node CreateCopy()
    {
        return new EvaluatePathNode();
    }
}
