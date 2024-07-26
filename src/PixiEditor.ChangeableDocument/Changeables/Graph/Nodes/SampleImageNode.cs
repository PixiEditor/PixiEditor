using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("SampleImage")]
public class SampleImageNode : Node
{
    private Pixmap? pixmap;

    public InputProperty<Surface?> Image { get; }

    public FuncOutputProperty<VecD> Coordinate { get; }

    public FuncOutputProperty<Color> Color { get; }

    public override string DisplayName { get; set; } = "SAMPLE_IMAGE";

    public SampleImageNode()
    {
        Image = CreateInput<Surface>(nameof(Surface), "IMAGE", null);
        Coordinate = CreateFuncOutput(nameof(Coordinate), "UV", ctx => ctx.Position);
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
    }

    private Color GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        if (pixmap == null)
            return new Color();

        var x = context.Position.X * context.Size.X;
        var y = context.Position.Y * context.Size.Y;

        return pixmap.GetPixelColor((int)x, (int)y);
    }

    internal void PreparePixmap()
    {
        pixmap = Image.Value?.PeekPixels();
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        PreparePixmap();
        return Image.Value;
    }

    public override Node CreateCopy() => new SampleImageNode();
}
