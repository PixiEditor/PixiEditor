namespace PixiEditor.ChangeableDocument.Enums;

public enum MathNodeMode
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Sin,
    Cos,
    Tan,
}

public static class MathNodeModeExtensions
{
    public static bool UsesYValue(this MathNodeMode mode) => !(mode is >= MathNodeMode.Sin and <= MathNodeMode.Tan);
}
