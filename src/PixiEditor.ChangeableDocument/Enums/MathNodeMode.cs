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
    [Description("GREATER_THAN")]
    GreaterThan,
    [Description("GREATER_THAN_OR_EQUAL")]
    GreaterThanOrEqual,
    [Description("LESS_THAN")]
    LessThan,
    [Description("LESS_THAN_OR_EQUAL")]
    LessThanOrEqual,
    [Description("COMPARE")]
    Compare
}

public static class MathNodeModeExtensions
{
    public static bool UsesYValue(this MathNodeMode mode) => !(mode is >= MathNodeMode.Sin and <= MathNodeMode.Tan);

    public static bool UsesZValue(this MathNodeMode mode) => mode is MathNodeMode.Compare;

    public static (string x, string y, string z) GetNaming(this MathNodeMode mode) => mode switch
    {
        MathNodeMode.Compare => ("VALUE", "TARGET", "EPSILON"),
        _ => ("X", "Y", "Z")
    };
}
