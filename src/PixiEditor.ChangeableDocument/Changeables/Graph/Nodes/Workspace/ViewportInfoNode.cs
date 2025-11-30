using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;

[NodeInfo("ViewportInfo")]
public class ViewportInfoNode : Node
{
    public OutputProperty<Matrix3X3> Transform { get; }
    public OutputProperty<VecD> PanPosition { get; }
    public OutputProperty<double> Zoom { get; }
    public OutputProperty<bool> FlipX { get; }
    public OutputProperty<bool> FlipY { get; }

    public ViewportInfoNode()
    {
        Transform = CreateOutput<Matrix3X3>("Transform", "TRANSFORM", Matrix3X3.Identity);
        PanPosition = CreateOutput<VecD>("PanPosition", "PAN_POSITION", VecD.Zero);
        Zoom = CreateOutput<double>("Zoom", "ZOOM", 1.0);
        FlipX = CreateOutput<bool>("FlipX", "FLIP_X", false);
        FlipY = CreateOutput<bool>("FlipY", "FLIP_Y", false);
    }

    protected override void OnExecute(RenderContext context)
    {
        Transform.Value = context.ViewportData.Transform;
        PanPosition.Value = context.ViewportData.Translation;
        Zoom.Value = context.ViewportData.Zoom;
        FlipX.Value = context.ViewportData.FlipX;
        FlipY.Value = context.ViewportData.FlipY;
    }

    public override Node CreateCopy()
    {
        return new ViewportInfoNode();
    }
}
