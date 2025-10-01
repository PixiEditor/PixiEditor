using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class ControlPointHandle : Handle
{
    public Handle ConnectedTo { get; set; }

    public override VecD HitSizeMargin { get; set; } = new VecD(10);

    public ControlPointHandle(IOverlay owner) : base(owner)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
    }

    protected override void OnDraw(Canvas target)
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

        if (ConnectedTo != null)
        {
            target.DrawLine(Position, ConnectedTo.Position, FillPaint);
        }
    }
}
