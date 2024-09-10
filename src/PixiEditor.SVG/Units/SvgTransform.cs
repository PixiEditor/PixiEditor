using System.Numerics;

namespace PixiEditor.SVG.Units;

public struct SvgTransform : ISvgUnit
{
    public SvgTransform()
    {
    }

    public Matrix3x2 MatrixValue { get; set; } = Matrix3x2.Identity;
}
