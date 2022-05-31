using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Enums;
internal static class SelectionModeEx
{
    public static SKPathOp ToSKPathOp(this SelectionMode mode)
    {
        return mode switch
        {
            SelectionMode.New => throw new ArgumentException("The New mode has no corresponding operation"),
            SelectionMode.Add => SKPathOp.Union,
            SelectionMode.Subtract => SKPathOp.Difference,
            SelectionMode.Intersect => SKPathOp.Intersect,
            _ => throw new NotImplementedException(),
        };
    }
}
