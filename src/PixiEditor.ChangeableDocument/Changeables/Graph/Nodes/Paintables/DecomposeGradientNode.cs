using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Paintables;

[NodeInfo("DecomposeGradient")]
public class DecomposeGradientNode : Node
{
    public InputProperty<GradientPaintable> Gradient { get; }
    public OutputProperty<Color[]> Colors { get; }
    public OutputProperty<float[]> Offsets { get; }
    public OutputProperty<GradientType> Type { get; }
    public OutputProperty<bool> AbsoluteCoordinates { get; }
    public OutputProperty<VecD> StartPoint { get; }
    public OutputProperty<VecD> EndPoint { get; }
    public OutputProperty<VecD> CenterPoint { get; }
    public OutputProperty<double> Radius { get; }
    public OutputProperty<double> Angle { get; }

    public DecomposeGradientNode()
    {
        Gradient = CreateInput<GradientPaintable>("Gradient", "GRADIENT", null);
        Colors = CreateOutput<Color[]>("Colors", "COLORS", null);
        Offsets = CreateOutput<float[]>("Offsets", "OFFSETS", null);
        Type = CreateOutput<GradientType>("Type", "TYPE", GradientType.Linear);
        AbsoluteCoordinates = CreateOutput<bool>("AbsoluteCoordinates", "ABSOLUTE_COORDINATES", false);
        StartPoint = CreateOutput<VecD>("StartPoint", "START_POINT", VecD.Zero);
        EndPoint = CreateOutput<VecD>("EndPoint", "END_POINT", new VecD(1, 0));
        CenterPoint = CreateOutput<VecD>("CenterPoint", "CENTER_POINT", new VecD(0.5, 0.5));
        Radius = CreateOutput<double>("Radius", "RADIUS", 0);
        Angle = CreateOutput<double>("Angle", "ANGLE", 0);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Gradient.Value != null)
        {
            Colors.Value = Gradient.Value.GradientStops.Select(x => x.Color).ToArray();
            Offsets.Value = Gradient.Value.GradientStops.Select(x => (float)x.Offset).ToArray();
            AbsoluteCoordinates.Value = Gradient.Value.AbsoluteValues;
            Radius.Value = 0;
            Angle.Value = 0;
            StartPoint.Value = VecD.Zero;
            EndPoint.Value = new VecD(1, 0);
            CenterPoint.Value = new VecD(0.5, 0.5);

            if (Gradient.Value is LinearGradientPaintable linear)
            {
                Type.Value = GradientType.Linear;
                StartPoint.Value = linear.Start;
                EndPoint.Value = linear.End;
            }
            else if (Gradient.Value is RadialGradientPaintable radial)
            {
                Type.Value = GradientType.Radial;
                CenterPoint.Value = radial.Center;
                Radius.Value = radial.Radius;
            }
            else if (Gradient.Value is SweepGradientPaintable sweep)
            {
                Type.Value = GradientType.Conical;
                CenterPoint.Value = sweep.Center;
                Angle.Value = sweep.Angle;
            }
        }
    }

    public override Node CreateCopy()
    {
        return new DecomposeGradientNode();
    }
}
