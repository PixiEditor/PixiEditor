using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("CreateImage")]
public class CreateImageNode : Node, IPreviewRenderable
{
    public OutputProperty<Texture> Output { get; }

    public InputProperty<VecI> Size { get; }

    public InputProperty<Paintable> Fill { get; }

    public RenderInputProperty Content { get; }

    public InputProperty<Matrix3X3> ContentMatrix { get; }


    public RenderOutputProperty RenderOutput { get; }

    private TextureCache textureCache = new();

    public CreateImageNode()
    {
        Output = CreateOutput<Texture>(nameof(Output), "IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32)).WithRules(v => v.Min(VecI.One));
        Fill = CreateInput<Paintable>(nameof(Fill), "FILL", new ColorPaintable(Colors.Transparent));
        Content = CreateRenderInput(nameof(Content), "CONTENT");
        ContentMatrix = CreateInput<Matrix3X3>(nameof(ContentMatrix), "MATRIX", Matrix3X3.Identity);
        RenderOutput = CreateRenderOutput("RenderOutput", "RENDER_OUTPUT", () => new Painter(OnPaint));
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return;
        }

        var surface = Render(context);

        Output.Value = surface;

        RenderOutput.ChainToPainterValue();
    }

    private Texture Render(RenderContext context)
    {
        var surface = textureCache.RequestTexture(0, (VecI)(Size.Value * context.ChunkResolution.Multiplier()), context.ProcessingColorSpace, false);
        surface.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);

        if (Fill.Value is ColorPaintable colorPaintable)
        {
            surface.DrawingSurface.Canvas.Clear(colorPaintable.Color);
        }
        else
        {
            using Paint paint = new Paint();
            paint.SetPaintable(Fill.Value);
            surface.DrawingSurface.Canvas.DrawRect(0, 0, Size.Value.X, Size.Value.Y, paint);
        }

        int saved = surface.DrawingSurface.Canvas.Save();

        RenderContext ctx = context.Clone();
        ctx.RenderSurface = surface.DrawingSurface;
        ctx.RenderOutputSize = surface.Size;

        float chunkMultiplier = (float)context.ChunkResolution.Multiplier();

        surface.DrawingSurface.Canvas.SetMatrix(
            surface.DrawingSurface.Canvas.TotalMatrix.Concat(
                Matrix3X3.CreateScale(chunkMultiplier, chunkMultiplier).Concat(ContentMatrix.Value)));

        Content.Value?.Paint(ctx, surface.DrawingSurface);

        surface.DrawingSurface.Canvas.RestoreToCount(saved);
        return surface;
    }

    private void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if(Output.Value == null || Output.Value.IsDisposed) return;

        int saved = surface.Canvas.Save();
        surface.Canvas.Scale((float)context.ChunkResolution.InvertedMultiplier());
        surface.Canvas.DrawSurface(Output.Value.DrawingSurface, 0, 0);

        surface.Canvas.RestoreToCount(saved);
    }

    public override Node CreateCopy() => new CreateImageNode();

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose();
    }

    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return null;
        }

        return new RectD(0, 0, Size.Value.X, Size.Value.Y);
    }

    public bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return false;
        }

        if (Output.Value == null)
        {
            return false;
        }

        var surface = Render(context);
        
        if (surface == null || surface.IsDisposed)
        {
            return false;
        }
        
        renderOn.Canvas.DrawSurface(surface.DrawingSurface, 0, 0);
        
        return true;
    }
}
