using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class ControlPointHandle : Handle
{
    public VecF ConnectToPosition { get; set; }

    public ControlPointHandle(IOverlay owner) : base(owner)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
    }

    public override void Draw(Canvas target)
    {
        float radius = (float)(Size.X / 2);
        radius /= (float)ZoomScale;
        if (FillPaint != null)
        {
            target.DrawCircle(Position, radius, FillPaint);
        }

        if (StrokePaint != null)
        {
            target.DrawCircle(Position, radius, StrokePaint);
        }

        target.DrawLine(Position, (VecD)ConnectToPosition, FillPaint);
    }
}
