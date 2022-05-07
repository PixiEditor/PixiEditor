using System.Windows;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal class AffineMode : ITransformMode
{
    private TransformOverlay owner;

    private static Pen blackPen = new Pen(Brushes.Black, 1);
    private static Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 6 }, 0) };

    public AffineMode(TransformOverlay owner)
    {
        this.owner = owner;
    }

    public void OnRender(DrawingContext context)
    {
        blackPen.Thickness = 1 / owner.ZoomboxScale;
        blackDashedPen.Thickness = 1 / owner.ZoomboxScale;

        var trans = owner.AffineTransform;
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(trans.TopLeft), TransformOverlay.ToPoint(trans.TopRight));
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(trans.TopLeft), TransformOverlay.ToPoint(trans.BottomLeft));
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(trans.BottomRight), TransformOverlay.ToPoint(trans.BottomLeft));
        context.DrawLine(blackDashedPen, TransformOverlay.ToPoint(trans.BottomRight), TransformOverlay.ToPoint(trans.TopRight));

        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(trans.TopLeft));
        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(trans.TopRight));
        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(trans.BottomLeft));
        context.DrawRectangle(Brushes.White, blackPen, owner.ToRect(trans.BottomRight));

        Vector2d rotPos = GetRotPos();
        double radius = TransformOverlay.SideLength / owner.ZoomboxScale / 2;
        context.DrawEllipse(Brushes.White, blackPen, new Point(rotPos.X, rotPos.Y), radius, radius);
    }

    public void OnAnchorDrag(Vector2d pos, Anchor anchor)
    {
        if (anchor == Anchor.Rotation)
        {
            var cur = GetRotPos();
            var angle = (cur - owner.AffineTransform.Center).CCWAngleTo(pos - owner.AffineTransform.Center);
            owner.AffineTransform = new AffineTransform(owner.AffineTransform.Center, owner.AffineTransform.Size, owner.AffineTransform.Angle + angle);
            return;
        }

        Vector2d curPos = AnchorTransformSpace(anchor);
        Vector2d newPos = (pos - owner.AffineTransform.Center).Rotate(-owner.AffineTransform.Angle);

        Anchor opposite = GetOpposite(anchor);
        Vector2d oppPos = AnchorTransformSpace(opposite);

        Vector2d oldSize = curPos - oppPos;
        Vector2d newSize = newPos - oppPos;

        Vector2d deltaCenter = (newSize - oldSize).Rotate(owner.AffineTransform.Angle) / 2;
        owner.AffineTransform = new AffineTransform(owner.AffineTransform.Center + deltaCenter, newSize.Abs(), owner.AffineTransform.Angle);
    }

    private Vector2d GetRotPos()
    {
        var trans = owner.AffineTransform;
        return (trans.TopLeft + trans.TopRight) / 2 + (trans.TopLeft - trans.BottomLeft).Normalize() * 10 / owner.ZoomboxScale;
    }

    private Vector2d AnchorTransformSpace(Anchor anchor)
    {
        var halfSize = owner.AffineTransform.Size / 2;
        return anchor switch
        {
            Anchor.TopLeft => -halfSize,
            Anchor.TopRight => new(halfSize.X, -halfSize.Y),
            Anchor.BottomLeft => new(-halfSize.X, halfSize.Y),
            Anchor.BottomRight => halfSize,
            _ => throw new System.NotImplementedException(),
        };
    }

    private Anchor GetOpposite(Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => Anchor.BottomRight,
            Anchor.TopRight => Anchor.BottomLeft,
            Anchor.BottomLeft => Anchor.TopRight,
            Anchor.BottomRight => Anchor.TopLeft,
            _ => throw new System.NotImplementedException(),
        };
    }

    public Anchor? GetAnchorInPosition(Vector2d pos)
    {
        if (owner.IsWithinAnchor(owner.AffineTransform.TopLeft, pos))
            return Anchor.TopLeft;
        if (owner.IsWithinAnchor(owner.AffineTransform.TopRight, pos))
            return Anchor.TopRight;
        if (owner.IsWithinAnchor(owner.AffineTransform.BottomLeft, pos))
            return Anchor.BottomLeft;
        if (owner.IsWithinAnchor(owner.AffineTransform.BottomRight, pos))
            return Anchor.BottomRight;
        if (owner.IsWithinAnchor(GetRotPos(), pos))
            return Anchor.Rotation;
        return null;
    }
}
