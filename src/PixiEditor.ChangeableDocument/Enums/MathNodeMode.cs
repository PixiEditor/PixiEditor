using System.ComponentModel;

namespace PixiEditor.ChangeableDocument.Enums;

public enum MathNodeMode
{
    [Description("MATH_ADD")]
    Add,
    [Description("MATH_SUBTRACT")]
    Subtract,
    [Description("MULTIPLY")]
    Multiply,
    [Description("DIVIDE")]
    Divide,
    [Description("SIN")]
    Sin,
    [Description("COS")]
    Cos,
    [Description("TAN")]
    Tan,
}

public static class MathNodeModeExtensions
{
    public static bool UsesYValue(this MathNodeMode mode) => !(mode is >= MathNodeMode.Sin and <= MathNodeMode.Tan);
}
