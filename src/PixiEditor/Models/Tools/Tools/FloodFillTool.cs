using System.Windows.Input;
using PixiEditor.Models.Commands.Attributes;
using SkiaSharp;

namespace PixiEditor.Models.Tools.Tools;

[Command.Tool(Key = Key.G)]
internal class FloodFillTool : BitmapOperationTool
{
    private SKPaint fillPaint = new SKPaint() { BlendMode = SKBlendMode.Src };

    public FloodFillTool()
    {
        ActionDisplay = "Press on an area to fill it.";
    }

    public override string Tooltip => $"Fills area with color. ({Shortcut})";

    public override void Use()
    {
        throw new NotImplementedException();
    }
}
