using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public static class LineHelper
{
    public static void GetInterpolatedPointsNonAlloc(VecD start, VecD end, List<VecD> outputList)
    {
        VecD delta = end - start;
        double longest = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));

        // ensure at least 2 points and cap excessive lengths
        int count = Math.Clamp((int)Math.Ceiling(longest) + 1, 2, 100000);

        outputList.Clear();
        for (int i = 0; i < count; i++)
        {
            double t = (double)i / (count - 1);
            outputList.Add(start + delta * t);
        }
    }
}
