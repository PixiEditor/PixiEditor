using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("CreateImage")]
public class CreateImageNode : Node
{
    public OutputProperty<Texture> Output { get; }

    public InputProperty<VecI> Size { get; }

    public InputProperty<Color> Fill { get; }

    public RenderInputProperty Content { get; }

    public RenderOutputProperty RenderOutput { get; }

    public CreateImageNode()
    {
        Output = CreateOutput<Texture>(nameof(Output), "EMPTY_IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32)).WithRules(v => v.Min(VecI.One));
        Fill = CreateInput(nameof(Fill), "FILL", Colors.Transparent);
        Content = CreateRenderInput(nameof(Content), "CONTENT");
        RenderOutput = CreateRenderOutput("RenderOutput", "RENDER_OUTPUT", () => new Painter(OnPaint));
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return;
        }

        var surface = RequestTexture(0, Size.Value, false);

        surface.DrawingSurface.Canvas.Clear(Fill.Value);

        int saved = surface.DrawingSurface.Canvas.Save();

        RenderContext ctx = new RenderContext(surface.DrawingSurface, context.FrameTime, context.ChunkResolution,
            context.DocumentSize);

        Content.Value?.Paint(ctx, surface.DrawingSurface);

        surface.DrawingSurface.Canvas.RestoreToCount(saved);
        Output.Value = surface;


        RenderOutput.ChainToPainterValue();
    }

    private void OnPaint(RenderContext context, DrawingSurface surface)
    {
        surface.Canvas.DrawSurface(Output.Value.DrawingSurface, 0, 0);
    }

    public override Node CreateCopy() => new CreateImageNode();
}
