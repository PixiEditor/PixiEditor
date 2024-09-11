using System.Numerics;

namespace PixiEditor.SVG.Units;

public struct SvgTransform : ISvgUnit
{
    public SvgTransform()
    {
    }

    public Matrix3x2 MatrixValue { get; set; } = Matrix3x2.Identity;
    public string ToXml()
    {
        return $"matrix({MatrixValue.M11},{MatrixValue.M12},{MatrixValue.M21},{MatrixValue.M22},{MatrixValue.M31},{MatrixValue.M32})";
    }
}
