using Avalonia;

namespace PixiEditor.Helpers.Extensions;

public static class MatrixExtensions
{
    public static Matrix RotateAt(this Matrix matrix, double angle, double centerX, double centerY)
    {
        angle %= 360.0; // Doing the modulo before converting to radians reduces total error
        matrix *= CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY);
        return matrix;
    }

    public static Matrix ScaleAt(this Matrix matrix, double scaleX, double scaleY, double centerX, double centerY)
    {
        matrix *= CreateScaling(scaleX, scaleY, centerX, centerY);
        return matrix;
    }

    private static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
    {
        return new Matrix(scaleX, 0, 0, scaleY, centerX - scaleX*centerX, centerY - scaleY * centerY);
    }

    private static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
    {
        double sin = Math.Sin(angle);
        double cos = Math.Cos(angle);
        double dx = (centerX * (1.0 - cos)) + (centerY * sin);
        double dy = (centerY * (1.0 - cos)) - (centerX * sin);

        return new Matrix(cos, sin, -sin, cos, dx, dy);
    }
}
