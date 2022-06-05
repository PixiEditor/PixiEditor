using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations;
public class EllipseHelper
{
    public static (List<VecI> lines, RectI rect) SplitEllipseIntoRegions(List<VecI> ellipse, RectI ellipseBounds)
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
            if (!added[point.Y - ellipseBounds.Top] && i > 0 && ellipse[i - 1].Y == point.Y && point.X - ellipse[i - 1].X > 1)
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
    public static List<VecI> GenerateEllipseFromRect(RectI rect)
    {
        if (rect.IsZeroOrNegativeArea)
            return new();
        float radiusX = (rect.Width - 1) / 2.0f;
        float radiusY = (rect.Height - 1) / 2.0f;
        return GenerateMidpointEllipse(radiusX, radiusY, rect.Center.X, rect.Center.Y);
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
    public static List<VecI> GenerateMidpointEllipse(
        double halfWidth,
        double halfHeight,
        double centerX,
        double centerY,
        List<VecI>? listToFill = null)
    {
        listToFill ??= new List<VecI>();
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
        }
        while (currentSlope > -1 && currentY - centerY > 0.5);

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

    private static void AddFallbackRectangle(double halfWidth, double halfHeight, double centerX, double centerY, List<VecI> coordinates)
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

    private static void AddRegionPoints(List<VecI> coordinates, double x, double xc, double y, double yc)
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
}
