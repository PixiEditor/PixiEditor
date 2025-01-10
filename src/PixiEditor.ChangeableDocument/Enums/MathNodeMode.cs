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
    Compare,
    [Description("MATH_POWER")]
    Power,
    [Description("LOGARITHM")]
    Logarithm,
    [Description("NATURAL_LOGARITHM")]
    NaturalLogarithm,
    [Description("ROOT")]
    Root,
    [Description("INVERSE_ROOT")]
    InverseRoot,
    [Description("FRACTION")]
    Fraction,
    [Description("ABSOLUTE")]
    Absolute,
    [Description("NEGATE")]
    Negate,
    [Description("FLOOR")]
    Floor,
    [Description("CEIL")]
    Ceil,
    [Description("ROUND")]
    Round,
    [Description("MODULO")]
    Modulo,
    [Description("MIN")]
    Min,
    [Description("MAX")]
    Max,
    [Description("STEP")]
    Step,
    [Description("SMOOTH_STEP")]
    SmoothStep,
}

public static class MathNodeModeExtensions
{
    public static bool UsesYValue(this MathNodeMode mode) =>
        mode != MathNodeMode.Sin &&
        mode != MathNodeMode.Cos &&
        mode != MathNodeMode.Tan &&
        mode != MathNodeMode.Fraction &&
        mode != MathNodeMode.Absolute &&
        mode != MathNodeMode.Negate &&
        mode != MathNodeMode.Floor &&
        mode != MathNodeMode.Ceil &&
        mode != MathNodeMode.Round &&
        mode != MathNodeMode.NaturalLogarithm;


    public static bool UsesZValue(this MathNodeMode mode) =>
        mode is MathNodeMode.Compare or MathNodeMode.SmoothStep;

    public static (string x, string y, string z) GetNaming(this MathNodeMode mode) => mode switch
    {
        MathNodeMode.Compare => ("VALUE", "TARGET", "EPSILON"),
        _ => ("X", "Y", "Z")
    };
}
