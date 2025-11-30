using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public static class LineHelper
{
    public static VecD[] GetInterpolatedPoints(VecD start, VecD end)
    {
        VecD delta = end - start;
        double longest = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));

        // ensure at least 2 points and cap excessive lengths
        int count = Math.Clamp((int)Math.Ceiling(longest) + 1, 2, 100000);

        VecD[] output = new VecD[count];
        for (int i = 0; i < count; i++)
        {
            double t = (double)i / (count - 1);
            output[i] = start + delta * t;
        }

        return output;
    }
}
