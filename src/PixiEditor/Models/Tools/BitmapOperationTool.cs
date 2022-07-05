using SkiaSharp;

namespace PixiEditor.Models.Tools;

public abstract class BitmapOperationTool : Tool
{
    public abstract void Use();

    public override void BeforeUse()
    {
    }

    /// <summary>
    /// Executes undo adding procedure.
    /// </summary>
    /// <remarks>When overriding, set UseDefaultUndoMethod to false.</remarks>
    public override void AfterUse(SKRectI sessionRect)
    {
    }
}
