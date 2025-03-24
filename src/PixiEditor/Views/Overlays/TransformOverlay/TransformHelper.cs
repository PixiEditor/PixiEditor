#nullable enable

using Avalonia;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Overlays.TransformOverlay;

internal static class TransformHelper
{
    public static RectD ToHandleRect(VecD pos, VecD size, double zoomboxScale)
    {
        double scaledX = size.X / zoomboxScale;
        double scaledY = size.Y / zoomboxScale;
        return new RectD(pos.X - scaledX / 2, pos.Y - scaledY / 2, scaledX, scaledY);
    }

    public static VecD ToVecD(Point pos) => new VecD(pos.X, pos.Y);
    public static Point ToPoint(VecD vec) => new Point(vec.X, vec.Y);

    public static ShapeCorners AlignToPixels(ShapeCorners corners)
    {
        corners.TopLeft = corners.TopLeft.Round();
        corners.TopRight = corners.TopRight.Round();
        corners.BottomLeft = corners.BottomLeft.Round();
        corners.BottomRight = corners.BottomRight.Round();
        return corners;
    }

    public static Cursor GetResizeCursor(Anchor anchor, ShapeCorners corners, double zoomboxAngle)
    {
        double angle;
        if (IsSide(anchor))
        {
            var (left, right) = GetCornersOnSide(anchor);
            VecD leftPos = GetAnchorPosition(corners, left);
            VecD rightPos = GetAnchorPosition(corners, right);
            angle = (leftPos - rightPos).Angle + Math.PI / 2;
        }
        else if (IsCorner(anchor))
        {
            var (left, right) = GetNeighboringCorners(anchor);
            VecD leftPos = GetAnchorPosition(corners, left);
            VecD curPos = GetAnchorPosition(corners, anchor);
            VecD rightPos = GetAnchorPosition(corners, right);
            angle = ((curPos - leftPos).Normalize() + (curPos - rightPos).Normalize()).Angle;
        }
        else
        {
            return new Cursor(StandardCursorType.Arrow);
        }

        angle += zoomboxAngle;
        angle = Math.Round(angle * 4 / Math.PI);
        angle = (int)((angle % 8 + 8) % 8);
        if (angle is 0 or 4)
        {
            return new Cursor(StandardCursorType.SizeWestEast);
        }

        if (angle is 2 or 6)
        {
            return new Cursor(StandardCursorType.SizeNorthSouth);
        }

        if (angle is 1 or 5)
        {
            return new Cursor(StandardCursorType.BottomRightCorner);
        }

        return new Cursor(StandardCursorType.BottomLeftCorner);
    }

    private static double GetSnappingAngle(double angle)
    {
        return Math.Round(angle * 8 / (Math.PI * 2)) * (Math.PI * 2) / 8;
    }

    public static double FindSnappingAngle(ShapeCorners corners, double desiredAngle)
    {
        var desTop = (corners.TopLeft - corners.TopRight).Rotate(desiredAngle).Angle;
        var desRight = (corners.TopRight - corners.BottomRight).Rotate(desiredAngle).Angle;
        var desBottom = (corners.BottomRight - corners.BottomLeft).Rotate(desiredAngle).Angle;
        var desLeft = (corners.BottomLeft - corners.TopLeft).Rotate(desiredAngle).Angle;

        var deltaTop = GetSnappingAngle(desTop) - desTop;
        var deltaRight = GetSnappingAngle(desRight) - desRight;
        var deltaLeft = GetSnappingAngle(desLeft) - desLeft;
        var deltaBottom = GetSnappingAngle(desBottom) - desBottom;

        var minDelta = deltaTop;
        if (Math.Abs(minDelta) > Math.Abs(deltaRight))
            minDelta = deltaRight;
        if (Math.Abs(minDelta) > Math.Abs(deltaLeft))
            minDelta = deltaLeft;
        if (Math.Abs(minDelta) > Math.Abs(deltaBottom))
            minDelta = deltaBottom;
        return minDelta + desiredAngle;
    }

    public static VecD OriginFromCorners(ShapeCorners corners)
    {
        var maybeOrigin = TwoLineIntersection(
            GetAnchorPosition(corners, Anchor.Top),
            GetAnchorPosition(corners, Anchor.Bottom),
            GetAnchorPosition(corners, Anchor.Left),
            GetAnchorPosition(corners, Anchor.Right)
        );
        return maybeOrigin ?? corners.TopLeft.Lerp(corners.BottomRight, 0.5);
    }

    public static VecD? TwoLineIntersection(VecD line1Start, VecD line1End, VecD line2Start, VecD line2End)
    {
        const double epsilon = 0.0001;

        VecD line1delta = line1End - line1Start;
        VecD line2delta = line2End - line2Start;

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

    /// <summary>
    /// The first anchor would be on your left if you were standing on the side and looking inside the shape; the second anchor is to the right.
    /// </summary>
    public static (Anchor leftAnchor, Anchor rightAnchor) GetCornersOnSide(Anchor side)
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

    /// <summary>
    /// The first corner would be on your left if you were standing on the passed corner and looking inside the shape; the second corner is to the right.
    /// </summary>
    public static (Anchor, Anchor) GetNeighboringCorners(Anchor corner)
    {
        return corner switch
        {
            Anchor.TopLeft => (Anchor.TopRight, Anchor.BottomLeft),
            Anchor.TopRight => (Anchor.BottomRight, Anchor.TopLeft),
            Anchor.BottomLeft => (Anchor.TopLeft, Anchor.BottomRight),
            Anchor.BottomRight => (Anchor.BottomLeft, Anchor.TopRight),
            _ => throw new ArgumentException($"{corner} is not a corner anchor"),
        };
    }

    public static ShapeCorners UpdateCorner(ShapeCorners original, Anchor corner, VecD newPos)
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

    public static VecD GetAnchorPosition(ShapeCorners corners, Anchor anchor)
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

    public static Anchor? GetAnchorInPosition(VecD pos, ShapeCorners corners, VecD origin, double zoomboxScale,
        VecD size)
    {
        VecD topLeft = corners.TopLeft;
        VecD topRight = corners.TopRight;
        VecD bottomLeft = corners.BottomLeft;
        VecD bottomRight = corners.BottomRight;

        // corners
        if (IsWithinHandle(topLeft, pos, zoomboxScale, size))
            return Anchor.TopLeft;
        if (IsWithinHandle(topRight, pos, zoomboxScale, size))
            return Anchor.TopRight;
        if (IsWithinHandle(bottomLeft, pos, zoomboxScale, size))
            return Anchor.BottomLeft;
        if (IsWithinHandle(bottomRight, pos, zoomboxScale, size))
            return Anchor.BottomRight;

        // sides
        if (IsWithinHandle((bottomLeft - topLeft) / 2 + topLeft, pos, zoomboxScale, size))
            return Anchor.Left;
        if (IsWithinHandle((bottomRight - topRight) / 2 + topRight, pos, zoomboxScale, size))
            return Anchor.Right;
        if (IsWithinHandle((topLeft - topRight) / 2 + topRight, pos, zoomboxScale, size))
            return Anchor.Top;
        if (IsWithinHandle((bottomLeft - bottomRight) / 2 + bottomRight, pos, zoomboxScale, size))
            return Anchor.Bottom;

        // origin
        if (IsWithinHandle(origin, pos, zoomboxScale, size))
            return Anchor.Origin;
        return null;
    }

    public static bool IsWithinHandle(VecD anchorPos, VecD mousePos, double zoomboxScale, VecD size)
    {
        var delta = (anchorPos - mousePos).Abs();
        VecD scaled = size / zoomboxScale / 2;
        return delta.X < scaled.X && delta.Y < scaled.Y;
    }

    public static VecD GetHandlePos(ShapeCorners corners, double zoomboxScale, VecD size)
    {
        VecD max = new(
            Math.Max(Math.Max(corners.TopLeft.X, corners.TopRight.X),
                Math.Max(corners.BottomLeft.X, corners.BottomRight.X)),
            Math.Max(Math.Max(corners.TopLeft.Y, corners.TopRight.Y),
                Math.Max(corners.BottomLeft.Y, corners.BottomRight.Y)));
        return max + new VecD(size.X / zoomboxScale, size.Y / zoomboxScale);
    }

    public static (Anchor, Anchor) GetAdjacentAnchors(Anchor capturedAnchor)
    {
        return capturedAnchor switch
        {
            Anchor.TopLeft => (Anchor.Top, Anchor.Left),
            Anchor.TopRight => (Anchor.Top, Anchor.Right),
            Anchor.BottomLeft => (Anchor.Bottom, Anchor.Left),
            Anchor.BottomRight => (Anchor.Bottom, Anchor.Right),
            Anchor.Top => (Anchor.TopLeft, Anchor.TopRight),
            Anchor.Bottom => (Anchor.BottomLeft, Anchor.BottomRight),
            Anchor.Left => (Anchor.TopLeft, Anchor.BottomLeft),
            Anchor.Right => (Anchor.TopRight, Anchor.BottomRight),
            _ => throw new ArgumentException($"{capturedAnchor} is not a corner or a side"),
        };
    }

    public static Anchor GetOppositeAnchor(Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => Anchor.BottomRight,
            Anchor.TopRight => Anchor.BottomLeft,
            Anchor.BottomLeft => Anchor.TopRight,
            Anchor.BottomRight => Anchor.TopLeft,
            Anchor.Top => Anchor.Bottom,
            Anchor.Bottom => Anchor.Top,
            Anchor.Left => Anchor.Right,
            Anchor.Right => Anchor.Left,
            _ => throw new ArgumentException($"{anchor} is not a corner or a side"),
        };
    }

    public static bool RotationIsAlmostCardinal(double radians, double threshold = 0.03)
    {
        double normalized = Math.Abs(radians % (2 * Math.PI));
        double[] cardinals = { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2, 2 * Math.PI };
        return cardinals.Any(cardinal => Math.Abs(normalized - cardinal) < threshold);
    }

    public static VecD? GetClosestAnchorToPoint(VecD point, ShapeCorners corners)
    {
        var distances = new Dictionary<Anchor, double>
        {
            { Anchor.TopLeft, (point - corners.TopLeft).Length },
            { Anchor.TopRight, (point - corners.TopRight).Length },
            { Anchor.BottomLeft, (point - corners.BottomLeft).Length },
            { Anchor.BottomRight, (point - corners.BottomRight).Length },
            { Anchor.Left, (point - corners.LeftCenter).Length },
            { Anchor.Right, (point - corners.RightCenter).Length },
            { Anchor.Top, (point - corners.TopCenter).Length },
            { Anchor.Bottom, (point - corners.BottomCenter).Length },
        };

        var ordered = distances.OrderBy(pair => pair.Value).ToList();
        if (!ordered.Any())
            return null;

        var anchor = ordered.First().Key;
        return GetAnchorPosition(corners, anchor);
    }
}
