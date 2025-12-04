using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public static class BresenhamLineHelper
{
    public static VecI[] GetBresenhamLine(VecI start, VecI end)
    {
        int count = Math.Abs((start - end).LongestAxis) + 1;
        if (count > 100000)
            return [];
        VecI[] output = new VecI[count];
        CalculateBresenhamLine(start, end, output);
        return output;
    }

    private static void CalculateBresenhamLine(VecI start, VecI end, VecI[] output)
    {
        int index = 0;

        int x1 = start.X;
        int x2 = end.X;
        int y1 = start.Y;
        int y2 = end.Y;

        if (x1 == x2 && y1 == y2)
        {
            output[index] = start;
            return;
        }

        int d, dx, dy, ai, bi, xi, yi;
        int x = x1, y = y1;

        if (x1 < x2)
        {
            xi = 1;
            dx = x2 - x1;
        }
        else
        {
            xi = -1;
            dx = x1 - x2;
        }

        if (y1 < y2)
        {
            yi = 1;
            dy = y2 - y1;
        }
        else
        {
            yi = -1;
            dy = y1 - y2;
        }

        output[index] = new VecI(x, y);
        index++;

        if (dx > dy)
        {
            ai = (dy - dx) * 2;
            bi = dy * 2;
            d = bi - dx;

            while (x != x2)
            {
                if (d >= 0)
                {
                    x += xi;
                    y += yi;
                    d += ai;
                }
                else
                {
                    d += bi;
                    x += xi;
                }

                output[index] = new VecI(x, y);
                index++;
            }
        }
        else
        {
            ai = (dx - dy) * 2;
            bi = dx * 2;
            d = bi - dy;

            while (y != y2)
            {
                if (d >= 0)
                {
                    x += xi;
                    y += yi;
                    d += ai;
                }
                else
                {
                    d += bi;
                    y += yi;
                }

                output[index] = new VecI(x, y);
                index++;
            }
        }
    }
}
