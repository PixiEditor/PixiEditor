namespace PixiEditor.Common;

public static class MathEx
{
    public static double SmoothStep(double edge0, double edge1, double x)
    {
        x = Math.Clamp((x - edge0) / (edge1 - edge0), 0, 1);
        return x * x * (3 - 2 * x);
    }
}
