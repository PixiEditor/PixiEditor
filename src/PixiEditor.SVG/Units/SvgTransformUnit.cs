using System.Globalization;
using System.Numerics;
using Drawie.Backend.Core.Numerics;

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

    public void ValuesFromXml(string readerValue)
    {
        if (readerValue.StartsWith("matrix(") && readerValue.EndsWith(")"))
        {
            string[] values = readerValue[7..^1].Split(", ");
            if (values.Length == 6)
            {
                if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
                    float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float skewY) &&
                    float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float skewX) &&
                    float.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY) &&
                    float.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float translateX) &&
                    float.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out float translateY))
                {
                    MatrixValue = new Matrix3X3(scaleX, skewX, translateX, skewY, scaleY, translateY, 0, 0, 1);
                }
            }
        }
        else
        {
            // todo: parse other types of transformation syntax (rotate, translate, scale etc)
        }
    }
}
