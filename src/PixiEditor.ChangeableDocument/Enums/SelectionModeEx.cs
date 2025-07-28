using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Enums;
internal static class SelectionModeEx
{
    public static VectorPathOp ToVectorPathOp(this SelectionMode mode)
    {
        return mode switch
        {
            SelectionMode.New => throw new ArgumentException("The New mode has no corresponding operation"),
            SelectionMode.Add => VectorPathOp.Union,
            SelectionMode.Subtract => VectorPathOp.Difference,
            SelectionMode.Intersect => VectorPathOp.Intersect,
            _ => throw new NotImplementedException(),
        };
    }
}
