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
using PixiEditor.ChangeableDocument.Changeables.Graph.ColorSpaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("CreateImage")]
public class CreateImageNode : Node
{
    public OutputProperty<Texture> Output { get; }

    public InputProperty<VecI> Size { get; }

    public InputProperty<Paintable> Fill { get; }

    public RenderInputProperty Content { get; }

    public InputProperty<Matrix3X3> ContentMatrix { get; }
    public InputProperty<ColorSpaceType> ColorSpace { get; }

    public RenderOutputProperty RenderOutput { get; }

    private TextureCache textureCache = new();

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    public CreateImageNode()
    {
        Output = CreateOutput<Texture>(nameof(Output), "IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32)).WithRules(v => v.Min(VecI.One));
        Fill = CreateInput<Paintable>(nameof(Fill), "FILL", new ColorPaintable(Colors.Transparent));
        Content = CreateRenderInput(nameof(Content), "CONTENT");
        ContentMatrix = CreateInput<Matrix3X3>(nameof(ContentMatrix), "MATRIX", Matrix3X3.Identity);
        ColorSpace = CreateInput(nameof(ColorSpace), "COLOR_SPACE", ColorSpaceType.Inherit);
        RenderOutput = CreateRenderOutput("RenderOutput", "RENDER_OUTPUT", () => new Painter(OnPaint));
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return;
        }

        var surface = Render(context);
        RenderPreviews(surface, context);

        Output.Value = surface;

        RenderOutput.ChainToPainterValue();
    }

    private Texture? Render(RenderContext context)
    {
        var size = (VecI)(Size.Value * context.ChunkResolution.Multiplier());
        if (size.X <= 0 || size.Y <= 0)
        {
            return null;
        }

        int id = (Size.Value * context.ChunkResolution.Multiplier()).GetHashCode();
        var colorSpace = ColorSpace.Value == ColorSpaceType.Inherit ? context.ProcessingColorSpace : (ColorSpace.Value == ColorSpaceType.Srgb ? Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgb() : Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgbLinear());
        var surface = textureCache.RequestTexture(id, (VecI)(Size.Value * context.ChunkResolution.Multiplier()), colorSpace, false);
        surface.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);

        if (Fill.Value is ColorPaintable colorPaintable)
        {
            surface.DrawingSurface.Canvas.Clear(colorPaintable.Color);
        }
        else
        {
            using Paint paint = new Paint();
            using var fill = Fill.Value.Clone();
            paint.SetPaintable(fill);
            paint.BlendMode = BlendMode.Src;
            paint.PaintableMatrix = Matrix3X3.CreateScale((float)context.ChunkResolution.Multiplier(), (float)context.ChunkResolution.Multiplier());
            surface.DrawingSurface.Canvas.DrawRect(0, 0, Size.Value.X, Size.Value.Y, paint);
        }

        int saved = surface.DrawingSurface.Canvas.Save();

        RenderContext ctx = context.Clone();
        ctx.RenderSurface = surface.DrawingSurface.Canvas;
        ctx.RenderOutputSize = surface.Size;
        ctx.VisibleDocumentRegion = null;

        float chunkMultiplier = (float)context.ChunkResolution.Multiplier();

        surface.DrawingSurface.Canvas.SetMatrix(
            surface.DrawingSurface.Canvas.TotalMatrix.Concat(
                Matrix3X3.CreateScale(chunkMultiplier, chunkMultiplier).Concat(ContentMatrix.Value)));

        Content.Value?.Paint(ctx, surface.DrawingSurface.Canvas);

        surface.DrawingSurface.Canvas.RestoreToCount(saved);
        return surface;
    }

    private void OnPaint(RenderContext context, Canvas surface)
    {
        if (Output.Value == null || Output.Value.IsDisposed) return;

        int saved = surface.Save();
        surface.Scale((float)context.ChunkResolution.InvertedMultiplier());
        surface.DrawSurface(Output.Value.DrawingSurface, 0, 0);

        surface.RestoreToCount(saved);
    }

    private void RenderPreviews(Texture surface, RenderContext context)
    {
        if(surface == null) return;

        var previews = context.GetPreviewTexturesForNode(Id);
        if (previews is null) return;
        foreach (var request in previews)
        {
            var texture = request.Texture;
            if (texture is null) continue;

            int saved = texture.DrawingSurface.Canvas.Save();

            VecD scaling = PreviewUtility.CalculateUniformScaling(surface.Size, texture.Size);
            VecD offset = PreviewUtility.CalculateCenteringOffset(surface.Size, texture.Size, scaling);
            texture.DrawingSurface.Canvas.Translate((float)offset.X, (float)offset.Y);
            texture.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            var previewCtx =
                PreviewUtility.CreatePreviewContext(context, scaling, context.RenderOutputSize, texture.Size);

            texture.DrawingSurface.Canvas.Clear();
            texture.DrawingSurface.Canvas.DrawSurface(surface.DrawingSurface, 0, 0);
            texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    public override Node CreateCopy() => new CreateImageNode();

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose();
    }
}
