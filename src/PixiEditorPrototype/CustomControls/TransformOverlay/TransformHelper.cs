using System;
using System.Windows;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal static class TransformHelper
{
    public const double AnchorSize = 10;
    public const double MoveHandleSize = 16;

    private static Pen blackPen = new Pen(Brushes.Black, 1);
    private static Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 4 }, 0) };
    private static Pen whiteDashedPen = new Pen(Brushes.White, 1) { DashStyle = new DashStyle(new double[] { 2, 4 }, 2) };
    private static Pen blackFreqDashedPen = new Pen(Brushes.Black, 1) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };
    private static Pen whiteFreqDashedPen = new Pen(Brushes.White, 1) { DashStyle = new DashStyle(new double[] { 2, 2 }, 2) };

    private static PathGeometry handleGeometry = new()
    {
        FillRule = FillRule.Nonzero,
        Figures = (PathFigureCollection?)new PathFigureCollectionConverter()
            .ConvertFrom("M 0.50025839 0 0.4248062 0.12971572 0.34987079 0.25994821 h 0.1002584 V 0.45012906 H 0.25994831 V 0.34987066 L 0.12971577 0.42480604 0 0.5002582 0.12971577 0.57519373 0.25994831 0.65012926 V 0.5498709 H 0.45012919 V 0.74005175 H 0.34987079 L 0.42480619 0.87028439 0.50025839 1 0.57519399 0.87028439 0.65012959 0.74005175 H 0.54987119 V 0.5498709 H 0.74005211 V 0.65012926 L 0.87028423 0.57519358 1 0.5002582 0.87028423 0.42480604 0.74005169 0.34987066 v 0.1002584 H 0.54987077 V 0.25994821 h 0.1002584 L 0.5751938 0.12971572 Z"),
    };

    public static Rect ToAnchorRect(Vector2d pos, double zoomboxScale)
    {
        double scaled = AnchorSize / zoomboxScale;
        return new Rect(pos.X - scaled / 2, pos.Y - scaled / 2, scaled, scaled);
    }

    public static Rect ToHandleRect(Vector2d pos, double zoomboxScale)
    {
        double scaled = MoveHandleSize / zoomboxScale;
        return new Rect(pos.X - scaled / 2, pos.Y - scaled / 2, scaled, scaled);
    }

    public static Vector2d ToVector2d(Point pos) => new Vector2d(pos.X, pos.Y);
    public static Point ToPoint(Vector2d vec) => new Point(vec.X, vec.Y);

    public static Vector2d OriginFromCorners(ShapeCorners corners)
    {
        var maybeOrigin = TwoLineIntersection(
            GetAnchorPosition(corners, Anchor.Top),
            GetAnchorPosition(corners, Anchor.Bottom),
            GetAnchorPosition(corners, Anchor.Left),
            GetAnchorPosition(corners, Anchor.Right)
            );
        return maybeOrigin ?? corners.TopLeft.Lerp(corners.BottomRight, 0.5);
    }

    public static Vector2d? TwoLineIntersection(Vector2d line1Start, Vector2d line1End, Vector2d line2Start, Vector2d line2End)
    {
        const double epsilon = 0.0001;

        Vector2d line1delta = line1End - line1Start;
        Vector2d line2delta = line2End - line2Start;

        // both lines are vertical, no intersections
        if (Math.Abs(line1delta.X) < epsilon && Math.Abs(line2delta.X) < epsilon)
            return null;

        // y = mx + c
        double m1 = line1delta.Y / line1delta.X;
        double m2 = line2delta.Y / line2delta.X;

        // line 1 is vertical (m1 is infinity)
        if (Math.Abs(line1delta.X) < epsilon)
        {
            double c2 = line2Start.Y - line2Start.X * m2;
            return new(line1Start.X, m2 * line1Start.X + c2);
        }

        // line 2 is vertical
        if (Math.Abs(line2delta.X) < epsilon)
        {
            double c1 = line1Start.Y - line1Start.X * m1;
            return new(line2Start.X, m1 * line2Start.X + c1);
        }

        // lines are parallel
        if (Math.Abs(m1 - m2) < epsilon)
            return null;

        {
            double c1 = line1Start.Y - line1Start.X * m1;
            double c2 = line2Start.Y - line2Start.X * m2;
            double x = (c1 - c2) / (m2 - m1);
            return new(x, m1 * x + c1);
        }
    }

    public static bool IsCorner(Anchor anchor)
    {
        return anchor is Anchor.TopLeft or Anchor.TopRight or Anchor.BottomRight or Anchor.BottomLeft;
    }

    public static bool IsSide(Anchor anchor)
    {
        return anchor is Anchor.Left or Anchor.Right or Anchor.Top or Anchor.Bottom;
    }

    public static Anchor GetOpposite(Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => Anchor.BottomRight,
            Anchor.TopRight => Anchor.BottomLeft,
            Anchor.BottomLeft => Anchor.TopRight,
            Anchor.BottomRight => Anchor.TopLeft,
            Anchor.Top => Anchor.Bottom,
            Anchor.Left => Anchor.Right,
            Anchor.Right => Anchor.Left,
            Anchor.Bottom => Anchor.Top,
            _ => throw new ArgumentException($"{anchor} is not a corner or a side"),
        };
    }

    public static (Anchor, Anchor) GetCornersOnSide(Anchor side)
    {
        return side switch
        {
            Anchor.Left => (Anchor.TopLeft, Anchor.BottomLeft),
            Anchor.Right => (Anchor.BottomRight, Anchor.TopRight),
            Anchor.Top => (Anchor.TopRight, Anchor.TopLeft),
            Anchor.Bottom => (Anchor.BottomLeft, Anchor.BottomRight),
            _ => throw new ArgumentException($"{side} is not a side anchor"),
        };
    }

    public static (Anchor, Anchor) GetNeighboringCorners(Anchor corner)
    {
        return corner switch
        {
            Anchor.TopLeft => (Anchor.TopRight, Anchor.BottomLeft),
            Anchor.TopRight => (Anchor.TopLeft, Anchor.BottomRight),
            Anchor.BottomLeft => (Anchor.TopLeft, Anchor.BottomRight),
            Anchor.BottomRight => (Anchor.TopRight, Anchor.BottomLeft),
            _ => throw new ArgumentException($"{corner} is not a corner anchor"),
        };
    }

    public static ShapeCorners UpdateCorner(ShapeCorners original, Anchor corner, Vector2d newPos)
    {
        if (corner == Anchor.TopLeft)
            original.TopLeft = newPos;
        else if (corner == Anchor.BottomLeft)
            original.BottomLeft = newPos;
        else if (corner == Anchor.TopRight)
            original.TopRight = newPos;
        else if (corner == Anchor.BottomRight)
            original.BottomRight = newPos;
        else
            throw new ArgumentException($"{corner} is not a corner");
        return original;
    }

    public static Vector2d GetAnchorPosition(ShapeCorners corners, Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => corners.TopLeft,
            Anchor.BottomRight => corners.BottomRight,
            Anchor.TopRight => corners.TopRight,
            Anchor.BottomLeft => corners.BottomLeft,
            Anchor.Top => corners.TopLeft.Lerp(corners.TopRight, 0.5),
            Anchor.Bottom => corners.BottomLeft.Lerp(corners.BottomRight, 0.5),
            Anchor.Left => corners.TopLeft.Lerp(corners.BottomLeft, 0.5),
            Anchor.Right => corners.BottomRight.Lerp(corners.TopRight, 0.5),
            _ => throw new ArgumentException($"{anchor} is not a corner or a side"),
        };
    }

    public static Anchor? GetAnchorInPosition(Vector2d pos, ShapeCorners corners, Vector2d origin, double zoomboxScale)
    {
        Vector2d topLeft = corners.TopLeft;
        Vector2d topRight = corners.TopRight;
        Vector2d bottomLeft = corners.BottomLeft;
        Vector2d bottomRight = corners.BottomRight;

        // corners
        if (IsWithinAnchor(topLeft, pos, zoomboxScale))
            return Anchor.TopLeft;
        if (IsWithinAnchor(topRight, pos, zoomboxScale))
            return Anchor.TopRight;
        if (IsWithinAnchor(bottomLeft, pos, zoomboxScale))
            return Anchor.BottomLeft;
        if (IsWithinAnchor(bottomRight, pos, zoomboxScale))
            return Anchor.BottomRight;

        // sides
        if (IsWithinAnchor((bottomLeft - topLeft) / 2 + topLeft, pos, zoomboxScale))
            return Anchor.Left;
        if (IsWithinAnchor((bottomRight - topRight) / 2 + topRight, pos, zoomboxScale))
            return Anchor.Right;
        if (IsWithinAnchor((topLeft - topRight) / 2 + topRight, pos, zoomboxScale))
            return Anchor.Top;
        if (IsWithinAnchor((bottomLeft - bottomRight) / 2 + bottomRight, pos, zoomboxScale))
            return Anchor.Bottom;

        // origin
        if (IsWithinAnchor(origin, pos, zoomboxScale))
            return Anchor.Origin;
        return null;
    }

    public static bool IsWithinAnchor(Vector2d anchorPos, Vector2d mousePos, double zoomboxScale)
    {
        var delta = (anchorPos - mousePos).Abs();
        double scaled = AnchorSize / zoomboxScale / 2;
        return delta.X < scaled && delta.Y < scaled;
    }

    public static bool IsWithinTransformHandle(Vector2d handlePos, Vector2d mousePos, double zoomboxScale)
    {
        var delta = (handlePos - mousePos).Abs();
        double scaled = MoveHandleSize / zoomboxScale / 2;
        return delta.X < scaled && delta.Y < scaled;
    }

    public static void DrawOverlay
        (DrawingContext context, Vector2d size, ShapeCorners corners, Vector2d origin, double zoomboxScale)
    {
        // draw transparent background to enable mouse input everywhere
        context.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(0, 0), new Size(size.X, size.Y)));

        blackPen.Thickness = 1 / zoomboxScale;
        blackDashedPen.Thickness = 1 / zoomboxScale;
        whiteDashedPen.Thickness = 1 / zoomboxScale;
        blackFreqDashedPen.Thickness = 1 / zoomboxScale;
        whiteFreqDashedPen.Thickness = 1 / zoomboxScale;

        Vector2d topLeft = corners.TopLeft;
        Vector2d topRight = corners.TopRight;
        Vector2d bottomLeft = corners.BottomLeft;
        Vector2d bottomRight = corners.BottomRight;

        // lines
        context.DrawLine(blackDashedPen, ToPoint(topLeft), ToPoint(topRight));
        context.DrawLine(whiteDashedPen, ToPoint(topLeft), ToPoint(topRight));
        context.DrawLine(blackDashedPen, ToPoint(topLeft), ToPoint(bottomLeft));
        context.DrawLine(whiteDashedPen, ToPoint(topLeft), ToPoint(bottomLeft));
        context.DrawLine(blackDashedPen, ToPoint(bottomRight), ToPoint(bottomLeft));
        context.DrawLine(whiteDashedPen, ToPoint(bottomRight), ToPoint(bottomLeft));
        context.DrawLine(blackDashedPen, ToPoint(bottomRight), ToPoint(topRight));
        context.DrawLine(whiteDashedPen, ToPoint(bottomRight), ToPoint(topRight));

        // corner anchors
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect(topLeft, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect(topRight, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect(bottomLeft, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect(bottomRight, zoomboxScale));

        // side anchors
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect((topLeft - topRight) / 2 + topRight, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect((topLeft - bottomLeft) / 2 + bottomLeft, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect((bottomLeft - bottomRight) / 2 + bottomRight, zoomboxScale));
        context.DrawRectangle(Brushes.White, blackPen, ToAnchorRect((topRight - bottomRight) / 2 + bottomRight, zoomboxScale));

        // origin
        double radius = AnchorSize / zoomboxScale / 2;
        context.DrawEllipse(Brushes.Transparent, blackFreqDashedPen, ToPoint(origin), radius, radius);
        context.DrawEllipse(Brushes.Transparent, whiteFreqDashedPen, ToPoint(origin), radius, radius);

        // move handle
        Vector2d handlePos = GetDragHandlePos(corners, zoomboxScale);
        const double CrossSize = MoveHandleSize - 1;
        context.DrawRectangle(Brushes.White, blackPen, ToHandleRect(handlePos, zoomboxScale));
        handleGeometry.Transform = new MatrixTransform(
            0, CrossSize / zoomboxScale,
            CrossSize / zoomboxScale, 0,
            handlePos.X - CrossSize / (zoomboxScale * 2), handlePos.Y - CrossSize / (zoomboxScale * 2)
            );
        context.DrawGeometry(Brushes.Black, null, handleGeometry);
    }

    public static Vector2d GetDragHandlePos(ShapeCorners corners, double zoomboxScale)
    {
        Vector2d max = new(
            Math.Max(Math.Max(corners.TopLeft.X, corners.TopRight.X), Math.Max(corners.BottomLeft.X, corners.BottomRight.X)),
            Math.Max(Math.Max(corners.TopLeft.Y, corners.TopRight.Y), Math.Max(corners.BottomLeft.Y, corners.BottomRight.Y)));
        return max + new Vector2d(MoveHandleSize / zoomboxScale, MoveHandleSize / zoomboxScale);
    }
}
