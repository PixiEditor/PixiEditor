using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Numerics;

namespace ChunkyImageLib.Operations;

public class EllipseHelper
{
    /// <summary>
    /// Separates the ellipse's inner area into a bunch of horizontal lines and one big rectangle for drawing.
    /// </summary>
    public static (List<VecI> lines, RectI rect) SplitEllipseFillIntoRegions(IReadOnlyList<VecI> ellipse,
        RectI ellipseBounds)
    {
        if (ellipse.Count == 0)
            return (new(), RectI.Empty);
        List<VecI> lines = new();

        VecD ellipseCenter = ellipseBounds.Center;
        VecD inscribedRectSize = ellipseBounds.Size * Math.Sqrt(2) / 2;
        inscribedRectSize.X -= 2;
        inscribedRectSize.Y -= 2;
        RectI inscribedRect = (RectI)RectD.FromCenterAndSize(ellipseCenter, inscribedRectSize).RoundInwards();
        if (inscribedRect.IsZeroOrNegativeArea)
            inscribedRect = RectI.Empty;

        bool[] added = new bool[ellipseBounds.Height];
        for (var i = 0; i < ellipse.Count; i++)
        {
            var point = ellipse[i];
            if (!added[point.Y - ellipseBounds.Top] &&
                i > 0 &&
                ellipse[i - 1].Y == point.Y &&
                point.X - ellipse[i - 1].X > 1 &&
                point.Y > ellipseBounds.Top &&
                point.Y < ellipseBounds.Bottom - 1)
            {
                int fromX = ellipse[i - 1].X + 1;
                int toX = point.X;
                int y = ellipse[i - 1].Y;
                added[point.Y - ellipseBounds.Top] = true;
                if (y >= inscribedRect.Top && y < inscribedRect.Bottom)
                {
                    lines.Add(new VecI(fromX, y));
                    lines.Add(new VecI(inscribedRect.Left, y));
                    lines.Add(new VecI(inscribedRect.Right, y));
                    lines.Add(new VecI(toX, y));
                }
                else
                {
                    lines.Add(new VecI(fromX, y));
                    lines.Add(new VecI(toX, y));
                }
            }
        }

        return (lines, inscribedRect);
    }

    /// <summary>
    /// Splits the ellipse into a bunch of horizontal lines.
    /// The resulting list contains consecutive pairs of <see cref="VecI"/>s, each pair has one for the start of the line and one for the end.
    /// </summary>
    public static List<VecI> SplitEllipseIntoLines(HashSet<VecI> ellipse)
    {
        List<VecI> lines = new();
        var sorted = ellipse.OrderBy(
            a => a,
            Comparer<VecI>.Create((a, b) => a.Y != b.Y ? a.Y - b.Y : a.X - b.X)
        );

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        VecI? prev = null;
        foreach (var point in sorted)
        {
            if (prev.HasValue && point.Y != prev.Value.Y)
            {
                int prevY = prev.Value.Y;
                lines.Add(new(minX, prevY));
                lines.Add(new(maxX, prevY));
                minX = int.MaxValue;
                maxX = int.MinValue;
            }

            minX = Math.Min(point.X, minX);
            maxX = Math.Max(point.X, maxX);
            prev = point;
        }

        if (prev != null)
        {
            lines.Add(new(minX, prev.Value.Y));
            lines.Add(new(maxX, prev.Value.Y));
        }

        return lines;
    }

    public static HashSet<VecI> GenerateEllipseFromRect(RectI rect, double rotationRad = 0)
    {
        if (rect.IsZeroOrNegativeArea)
            return new();
        float radiusX = (rect.Width - 1) / 2.0f;
        float radiusY = (rect.Height - 1) / 2.0f;
        if (rotationRad == 0)
            return GenerateMidpointEllipse(radiusX, radiusY, rect.Center.X, rect.Center.Y);
        
        return GenerateMidpointEllipse(radiusX, radiusY, rect.Center.X, rect.Center.Y, rotationRad);
    }

    /// <summary>
    /// Draws an ellipse using it's center and radii
    ///
    /// Here is a usage example:
    /// Let's say you want an ellipse that's 3 pixels wide and 3 pixels tall located in the top right corner of the canvas
    /// It's center is at (1.5; 1.5). That's in the middle of a pixel
    /// The radii are both equal to 1. Notice that it's 1 and not 1.5, since we want the ellipse to land in the middle of the pixel, not outside of it.
    /// See desmos (note the inverted y axis): https://www.desmos.com/calculator/tq9uqg0hcq
    ///
    /// Another example:
    /// 4x4 ellipse in the top right corner of the canvas
    /// Center is at (2; 2). It's a place where 4 pixels meet
    /// Both radii are 1.5. Making them 2 would make the ellipse touch the edges of pixels, whereas we want it to stay in the middle
    /// </summary>
    public static HashSet<VecI> GenerateMidpointEllipse(
        double halfWidth,
        double halfHeight,
        double centerX,
        double centerY,
        HashSet<VecI>? listToFill = null)
    {
        listToFill ??= new HashSet<VecI>();
        if (halfWidth < 1 || halfHeight < 1)
        {
            AddFallbackRectangle(halfWidth, halfHeight, centerX, centerY, listToFill);
            return listToFill;
        }

        // ellipse formula: halfHeight^2 * x^2 + halfWidth^2 * y^2 - halfHeight^2 * halfWidth^2 = 0

        // Make sure we are always at the center of a pixel
        double currentX = Math.Ceiling(centerX - 0.5) + 0.5;
        double currentY = centerY + halfHeight;


        double currentSlope;

        // from PI/2 to PI/4
        do
        {
            AddRegionPoints(listToFill, currentX, centerX, currentY, centerY);

            // calculate next pixel coords
            currentX++;

            if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX, 2)) +
                (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY - 0.5, 2)) -
                (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) >= 0)
            {
                currentY--;
            }

            // calculate how far we've advanced
            double derivativeX = 2 * Math.Pow(halfHeight, 2) * (currentX - centerX);
            double derivativeY = 2 * Math.Pow(halfWidth, 2) * (currentY - centerY);
            currentSlope = -(derivativeX / derivativeY);
        } while (currentSlope > -1 && currentY - centerY > 0.5);

        // from PI/4 to 0
        while (currentY - centerY >= 0)
        {
            AddRegionPoints(listToFill, currentX, centerX, currentY, centerY);

            currentY--;
            if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX + 0.5, 2)) +
                (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY, 2)) -
                (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) < 0)
            {
                currentX++;
            }
        }

        return listToFill;
    }

    // This function works, but honestly Skia produces better results, and it doesn't require so much
    // computation on the CPU. I'm leaving this, because once I (or someone else) figure out how to
    // make it better, and it will be useful.
    // Desmos with all the math https://www.desmos.com/calculator/m9lgg7s9zu
    private static HashSet<VecI> GenerateMidpointEllipse(double halfWidth, double halfHeight, double centerX,
        double centerY, double rotationRad)
    {
        var listToFill = new HashSet<VecI>();

        // formula ((x - h)cos(tetha) + (y - k)sin(tetha))^2 / a^2 + (-(x-h)sin(tetha)+(y-k)cos(tetha))^2 / b^2 = 1

        //double topMostTetha = GetTopMostAlpha(halfWidth, halfHeight, rotationRad);

        //VecD possiblyTopmostPoint = GetTethaPoint(topMostTetha, halfWidth, halfHeight, rotationRad);
        //VecD possiblyMinPoint = GetTethaPoint(topMostTetha + Math.PI, halfWidth, halfHeight, rotationRad);

        // less than, because y grows downwards
        //VecD actualTopmost = possiblyTopmostPoint.Y < possiblyMinPoint.Y ? possiblyTopmostPoint : possiblyMinPoint;
        
        //rotationRad = double.Round(rotationRad, 1);

        double currentTetha = 0;

        double tethaStep = 0.001;

        VecI[] lastPoints = new VecI[2];

        do
        {
            VecD point = GetTethaPoint(currentTetha, halfWidth, halfHeight, rotationRad);
            VecI floored = new((int)Math.Floor(point.X + centerX), (int)Math.Floor(point.Y + centerY));

            AddPoint(listToFill, floored, lastPoints);

            currentTetha += tethaStep;
        } while (currentTetha < Math.PI * 2);
        
        return listToFill;
    }

    private static void AddPoint(HashSet<VecI> listToFill, VecI floored, VecI[] lastPoints)
    {
        if(!listToFill.Add(floored)) return;

        if (lastPoints[0] == default)
        {
            lastPoints[0] = floored;
            return;
        }

        if (lastPoints[1] == default)
        {
            lastPoints[1] = floored;
            return;
        }

        if (IsLShape(lastPoints, floored))
        {
            listToFill.Remove(lastPoints[1]);

            lastPoints[0] = floored;
            lastPoints[1] = default;
            
            return;
        }

        lastPoints[0] = lastPoints[1];
        lastPoints[1] = floored;
    }

    private static bool IsLShape(VecI[] points, VecI third)
    {
        VecI first = points[0];
        VecI second = points[1];
        return first.X != third.X && first.Y != third.Y && (second - first).TaxicabLength == 1 &&
               (second - third).TaxicabLength == 1;
    }

    private static bool IsInsideEllipse(double x, double y, double centerX, double centerY, double halfWidth,
        double halfHeight, double rotationRad)
    {
        double lhs = Math.Pow(x * Math.Cos(rotationRad) + y * Math.Sin(rotationRad), 2) / Math.Pow(halfWidth, 2);
        double rhs = Math.Pow(-x * Math.Sin(rotationRad) + y * Math.Cos(rotationRad), 2) / Math.Pow(halfHeight, 2);

        return lhs + rhs <= 1;
    }

    private static VecD GetDerivative(double x, double halfWidth, double halfHeight, double rotationRad, double tetha)
    {
        double xDerivative = halfWidth * Math.Cos(tetha) * Math.Cos(rotationRad) -
                             halfHeight * Math.Sin(tetha) * Math.Sin(rotationRad);
        double yDerivative = halfWidth * Math.Cos(tetha) * Math.Sin(rotationRad) +
                             halfHeight * Math.Sin(tetha) * Math.Cos(rotationRad);

        return new VecD(xDerivative, yDerivative);
    }

    private static VecD GetTethaPoint(double alpha, double halfWidth, double halfHeight, double rotation)
    {
        double x =
            (halfWidth * Math.Cos(alpha) * Math.Cos(rotation) - halfHeight * Math.Sin(alpha) * Math.Sin(rotation));
        double y = halfWidth * Math.Cos(alpha) * Math.Sin(rotation) + halfHeight * Math.Sin(alpha) * Math.Cos(rotation);

        return new VecD(x, y);
    }

    private static double GetTopMostAlpha(double halfWidth, double halfHeight, double rotationRad)
    {
        if (rotationRad == 0)
            return 0;
        double tethaRot = Math.Cos(rotationRad) / Math.Sin(rotationRad);
        return Math.Atan((halfHeight * tethaRot) / halfWidth);
    }

    private static void AddFallbackRectangle(double halfWidth, double halfHeight, double centerX, double centerY,
        HashSet<VecI> coordinates)
    {
        int left = (int)Math.Floor(centerX - halfWidth);
        int top = (int)Math.Floor(centerY - halfHeight);
        int right = (int)Math.Floor(centerX + halfWidth);
        int bottom = (int)Math.Floor(centerY + halfHeight);

        for (int x = left; x <= right; x++)
        {
            coordinates.Add(new VecI(x, top));
            if (top != bottom)
                coordinates.Add(new VecI(x, bottom));
        }

        for (int y = top + 1; y < bottom; y++)
        {
            coordinates.Add(new VecI(left, y));
            if (left != right)
                coordinates.Add(new VecI(right, y));
        }
    }

    private static void AddRegionPoints(HashSet<VecI> coordinates, double x, double xc, double y, double yc)
    {
        int xFloor = (int)Math.Floor(x);
        int yFloor = (int)Math.Floor(y);
        int xFloorInv = (int)Math.Floor(-x + 2 * xc);
        int yFloorInv = (int)Math.Floor(-y + 2 * yc);

        //top and bottom or left and right
        if (xFloor == xFloorInv || yFloor == yFloorInv)
        {
            coordinates.Add(new VecI(xFloorInv, yFloorInv));
            coordinates.Add(new VecI(xFloor, yFloor));
        }
        //part of the arc
        else
        {
            coordinates.Add(new VecI(xFloorInv, yFloor));
            coordinates.Add(new VecI(xFloor, yFloor));
            coordinates.Add(new VecI(xFloorInv, yFloorInv));
            coordinates.Add(new VecI(xFloor, yFloorInv));
        }
    }

    public static VectorPath GenerateEllipseVectorFromRect(RectI location)
    {
        VectorPath path = new();
        path.AddOval(location);
       
        path.Close();
        
        return path;
    }
}
