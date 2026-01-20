using System.Globalization;
using System.Numerics;
using Drawie.Backend.Core.Numerics;
using PixiEditor.SVG.Elements;

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

    public string ToXml(DefStorage defs)
    {
        string translateX = MatrixValue.TransX.ToString(CultureInfo.InvariantCulture);
        string translateY = MatrixValue.TransY.ToString(CultureInfo.InvariantCulture);
        string scaleX = MatrixValue.ScaleX.ToString(CultureInfo.InvariantCulture);
        string scaleY = MatrixValue.ScaleY.ToString(CultureInfo.InvariantCulture);
        string skewX = MatrixValue.SkewX.ToString(CultureInfo.InvariantCulture);
        string skewY = MatrixValue.SkewY.ToString(CultureInfo.InvariantCulture);

        return $"matrix({scaleX}, {skewY}, {skewX}, {scaleY}, {translateX}, {translateY})";
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        if (readerValue.StartsWith("matrix(") && readerValue.EndsWith(")"))
        {
            string[] spaceSplitted = readerValue[7..^1].Split(" ");
            string[] commaSplitted = readerValue[7..^1].Replace(" ", "").Split(",");
            string[] values = spaceSplitted.Length == 6 ? spaceSplitted : commaSplitted;
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
            MatrixValue = TryParseDescriptiveTransform(readerValue);
        }
    }

    private Matrix3X3 TryParseDescriptiveTransform(string readerValue)
    {
        if (!readerValue.Contains('(') || !readerValue.Contains(')'))
        {
            return Matrix3X3.Identity;
        }

        string[] parts = readerValue.Split(')').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        Matrix3X3 result = Matrix3X3.Identity;
        for (int i = 0; i < parts.Length; i++)
        {
            string[] part = parts[i].Split('(');
            if (part.Length != 2)
            {
                continue;
            }

            result = result.Concat(ParsePart(part));
        }

        return result;
    }

    private static Matrix3X3 ParsePart(string[] part)
    {
        string transformType = part[0].Trim();
        string[] values = part[1].Split(' ', ')', ',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        if (values.Length == 0)
        {
            return Matrix3X3.Identity;
        }

        if (transformType == "translate")
        {
            if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float translateX))
            {
                float translateY = translateX;
                if (values.Length > 1)
                {
                    float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out translateY);
                }

                return Matrix3X3.CreateTranslation(translateX, translateY);
            }
        }
        else if (transformType == "scale")
        {
            if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX))
            {
                float scaleY = scaleX;
                if (values.Length > 1)
                {
                    float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out scaleY);
                }

                return Matrix3X3.CreateScale(scaleX, scaleY);
            }
        }
        else if (transformType == "rotate")
        {
            if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float angle))
            {
                float radians = angle * (float)Math.PI / 180;

                if (values.Length > 2)
                {
                    float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float centerX);
                    float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float centerY);
                    return Matrix3X3.CreateRotation(radians, centerX, centerY);
                }

                return Matrix3X3.CreateRotation(radians);
            }
        }
        else if (transformType == "skewX")
        {
            if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float skewX))
            {
                return Matrix3X3.CreateSkew(skewX, 0);
            }
        }
        else if (transformType == "skewY")
        {
            if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float skewY))
            {
                return Matrix3X3.CreateSkew(0, skewY);
            }
        }

        return Matrix3X3.Identity;
    }
}
