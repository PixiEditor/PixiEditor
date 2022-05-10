namespace PixiEditorPrototype.CustomControls.TransformOverlay;/*
internal class AffineMode : ITransformMode
{
    private TransformOverlay owner;

    public AffineMode(TransformOverlay owner)
    {
        this.owner = owner;
    }

    public void OnAnchorDrag(Vector2d pos, Anchor anchor)
    {
        switch (anchor)
        {
            case Anchor.Rotation:
                OnRotationDrag(pos);
                break;
            case Anchor.TopLeft:
            case Anchor.TopRight:
            case Anchor.BottomLeft:
            case Anchor.BottomRight:
                MoveCornerKeepStraight(pos, anchor);
                break;
            case Anchor.Top:
            case Anchor.Left:
            case Anchor.Right:
            case Anchor.Bottom:
                MoveSideKeepStraight(pos, anchor);
                break;
        }
    }

    private void MoveSideKeepStraight(Vector2d pos, Anchor anchor)
    {
        Vector2d curPos = AnchorInTransformSpace(anchor);
        Vector2d newPos = (pos - owner.AffineTransform.Center).Rotate(-owner.AffineTransform.Angle);

        if (anchor is Anchor.Bottom or Anchor.Top)
            newPos.X = curPos.X;
        else
            newPos.Y = curPos.Y;

        Vector2d topLeft = AnchorInTransformSpace(Anchor.TopLeft);
        Vector2d bottomRight = AnchorInTransformSpace(Anchor.BottomRight);
        Vector2d delta = newPos - curPos;

        if (anchor is Anchor.Top or Anchor.Left)
            topLeft += delta;
        else
            bottomRight += delta;

        Vector2d newTopLeft = new(Math.Min(topLeft.X, bottomRight.X), Math.Min(topLeft.Y, bottomRight.Y));
        Vector2d newBottomRight = new(Math.Max(topLeft.X, bottomRight.X), Math.Max(topLeft.Y, bottomRight.Y));

        Vector2d deltaCenter =
            ((newTopLeft - newBottomRight) / 2 + newBottomRight)
            .Rotate(owner.AffineTransform.Angle) / 2;

        Vector2d newSize = (newBottomRight - newTopLeft);

        owner.AffineTransform = new AffineTransform(owner.AffineTransform.Center + deltaCenter, newSize, owner.AffineTransform.Angle);
    }

    private void MoveCornerKeepStraight(Vector2d pos, Anchor anchor)
    {
        Vector2d curPos = AnchorInTransformSpace(anchor);
        Vector2d newPos = (pos - owner.AffineTransform.Center).Rotate(-owner.AffineTransform.Angle);

        Anchor opposite = GetOppositeCorner(anchor);
        Vector2d oppPos = AnchorInTransformSpace(opposite);

        Vector2d oldSize = curPos - oppPos;
        Vector2d newSize = newPos - oppPos;

        Vector2d deltaCenter = (newSize - oldSize).Rotate(owner.AffineTransform.Angle) / 2;
        owner.AffineTransform = new AffineTransform(owner.AffineTransform.Center + deltaCenter, newSize.Abs(), owner.AffineTransform.Angle);
    }

    private Vector2d AnchorInTransformSpace(Anchor anchor)
    {
        var halfSize = owner.AffineTransform.Size / 2;
        return anchor switch
        {
            Anchor.TopLeft => -halfSize,
            Anchor.TopRight => new(halfSize.X, -halfSize.Y),
            Anchor.BottomLeft => new(-halfSize.X, halfSize.Y),
            Anchor.BottomRight => halfSize,
            Anchor.Left => new(-halfSize.X, 0),
            Anchor.Right => new(halfSize.X, 0),
            Anchor.Top => new(0, -halfSize.Y),
            Anchor.Bottom => new(0, halfSize.Y),
            _ => throw new System.NotImplementedException(),
        };
    }

    private Anchor GetOppositeSide(Anchor side)
    {
        return side switch
        {
            Anchor.Left => Anchor.Right,
            Anchor.Right => Anchor.Left,
            Anchor.Top => Anchor.Bottom,
            Anchor.Bottom => Anchor.Top,
            _ => throw new ArgumentException($"{side} is not a side anchor"),
        };
    }

    private (Anchor,Anchor) GetCornersOnSide(Anchor side)
    {
        return side switch
        {
            Anchor.Left => (Anchor.TopLeft, Anchor.BottomLeft),
            Anchor.Right => (Anchor.TopRight, Anchor.BottomRight),
            Anchor.Top => (Anchor.TopLeft, Anchor.TopRight),
            Anchor.Bottom => (Anchor.BottomRight, Anchor.BottomLeft),
            _ => throw new ArgumentException($"{side} is not a side anchor"),
        };
    }
}
*/
