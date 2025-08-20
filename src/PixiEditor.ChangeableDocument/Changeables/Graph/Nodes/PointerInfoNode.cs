using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("PointerInfo")]
public class PointerInfoNode : Node
{
    public OutputProperty<double> Pressure { get; }
    public OutputProperty<double> Twist { get; }
    public OutputProperty<VecD> Tilt { get; }
    public OutputProperty<VecD> MovementDirection { get; }
    public OutputProperty<double> Rotation { get; }

    public PointerInfoNode()
    {
        Pressure = CreateOutput<double>("Pressure", "PRESSURE", 1.0);
        Twist = CreateOutput<double>("Twist", "TWIST", 0.0);
        Tilt = CreateOutput<VecD>("Tilt", "TILT", new VecD(0, 0));
        MovementDirection = CreateOutput<VecD>("MovementDirection", "MOVEMENT_DIRECTION", new VecD(0, 0));
        Rotation = CreateOutput<double>("Rotation", "ROTATION", 0.0);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (!context.FullRerender && context.PointerInfo.Equals(default))
        {
            return;
        }

        Pressure.Value = context.PointerInfo.Pressure;
        Twist.Value = context.PointerInfo.Twist;
        Tilt.Value = context.PointerInfo.Tilt;
        MovementDirection.Value = context.PointerInfo.MovementDirection;
        Rotation.Value = context.PointerInfo.Rotation;
    }

    public override Node CreateCopy()
    {
        return new PointerInfoNode();
    }
}
