using System;

namespace PixiEditor.Zoombox.Operations;

internal static class ZoomboxOperationHelper
{
    public static double Mod(double x, double m)
    {
        return (x % m + m) % m;
    }

    public static double SubtractOnCircle(double angle1, double angle2)
    {
        angle1 = Mod(angle1, Math.PI * 2);
        angle2 = Mod(angle2, Math.PI * 2);
        double diff = Mod(angle1 - angle2, Math.PI * 2);
        if (diff > Math.PI)
            diff -= Math.PI * 2;
        return diff;
    }
}
