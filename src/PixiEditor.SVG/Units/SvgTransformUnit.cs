using System.Globalization;
using System.Numerics;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.SVG.Units;

public struct SvgTransformUnit : ISvgUnit
{
    public SvgTransformUnit()
    {
    }

    public Matrix3X3 MatrixValue { get; set; } = Matrix3X3.Identity;
    
    public SvgTransformUnit(Matrix3X3 matrixValue)
    {
        MatrixValue = matrixValue;
    }
    
    public string ToXml()
    {
        string translateX = MatrixValue.TransX.ToString(CultureInfo.InvariantCulture);
        string translateY = MatrixValue.TransY.ToString(CultureInfo.InvariantCulture);
        string scaleX = MatrixValue.ScaleX.ToString(CultureInfo.InvariantCulture);
        string scaleY = MatrixValue.ScaleY.ToString(CultureInfo.InvariantCulture);
        string skewX = MatrixValue.SkewX.ToString(CultureInfo.InvariantCulture);
        string skewY = MatrixValue.SkewY.ToString(CultureInfo.InvariantCulture);
        
        return $"matrix({scaleX}, {skewY}, {skewX}, {scaleY}, {translateX}, {translateY})";
    }
}
