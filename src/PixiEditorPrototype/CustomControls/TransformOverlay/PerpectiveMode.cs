using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal class PerpectiveMode : ITransformMode
{
    private TransformOverlay owner;
    private static Pen blackPen = new Pen(Brushes.Black, 1);
    private static Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 6 }, 0) };

    public PerpectiveMode(TransformOverlay owner)
    {
        this.owner = owner;
    }

    public Anchor? GetAnchorInPosition(Vector2d pos)
    {
        if (owner.IsWithinAnchor(owner.PerspectiveTransform.TopLeft, pos))
            return Anchor.TopLeft;
        if (owner.IsWithinAnchor(owner.PerspectiveTransform.TopRight, pos))
            return Anchor.TopRight;
        if (owner.IsWithinAnchor(owner.PerspectiveTransform.BottomLeft, pos))
            return Anchor.BottomLeft;
        if (owner.IsWithinAnchor(owner.PerspectiveTransform.BottomRight, pos))
            return Anchor.BottomRight;
        return null;
    }

    public void OnAnchorDrag(Vector2d pos, Anchor anchor)
    {
        var trans = owner.PerspectiveTransform;
        if (anchor == Anchor.TopLeft)
            trans = trans with { TopLeft = pos };
        else if (anchor == Anchor.TopRight)
            trans = trans with { TopRight = pos };
        else if (anchor == Anchor.BottomLeft)
            trans = trans with { BottomLeft = pos };
        else if (anchor == Anchor.BottomRight)
            trans = trans with { BottomRight = pos };
        owner.PerspectiveTransform = trans;
    }

    public void OnRender(DrawingContext context)
    {
        blackPen.Thickness = 1 / owner.ZoomboxScale;
        blackDashedPen.Thickness = 1 / owner.ZoomboxScale;

        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(owner.PerspectiveTransform.TopLeft), TransformOverlay.ToPoint(owner.PerspectiveTransform.TopRight));
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(owner.PerspectiveTransform.TopLeft), TransformOverlay.ToPoint(owner.PerspectiveTransform.BottomLeft));
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(owner.PerspectiveTransform.BottomRight), TransformOverlay.ToPoint(owner.PerspectiveTransform.BottomLeft));
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(owner.PerspectiveTransform.BottomRight), TransformOverlay.ToPoint(owner.PerspectiveTransform.TopRight));

        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(owner.PerspectiveTransform.TopLeft));
        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(owner.PerspectiveTransform.TopRight));
        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(owner.PerspectiveTransform.BottomLeft));
        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(owner.PerspectiveTransform.BottomRight));
    }
}
