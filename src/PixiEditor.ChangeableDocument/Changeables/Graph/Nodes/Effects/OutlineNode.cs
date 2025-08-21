using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Effects;

[NodeInfo("Outline")]
public class OutlineNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<OutlineType> Type { get; }
    public InputProperty<double> Thickness { get; }
    public InputProperty<Color> Color { get; }

    private Kernel simpleKernel = new Kernel(3, 3, [1, 1, 1, 1, 1, 1, 1, 1, 1]);
    private Kernel pixelPerfectKernel = new Kernel(3, 3, [0, 1, 0, 1, -4, 1, 0, 1, 0]);
    private Kernel gaussianKernel = new Kernel(5, 5, [
        1, 4, 6, 4, 1,
        4, 16, 24, 16, 4,
        6, 24, 36, 24, 6,
        4, 16, 24, 16, 4,
        1, 4, 6, 4, 1
    ]);

    private Paint paint;
    private ImageFilter filter;

    private OutlineType? lastType = null;
    private VecI lastDocumentSize;

    protected override bool ExecuteOnlyOnCacheChange => true;

    public OutlineNode()
    {
        Background = CreateRenderInput("Background", "BACKGROUND");
        Type = CreateInput("Type", "TYPE", OutlineType.Simple);
        Thickness = CreateInput("Thickness", "THICKNESS", 1.0)
            .WithRules(validator => validator.Min(0.0));
        Color = CreateInput("Color", "COLOR", Colors.Black);

        paint = new Paint();

        Output.FirstInChain = null;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        lastDocumentSize = context.RenderOutputSize;

        Kernel finalKernel = Type.Value switch
        {
            OutlineType.Simple => simpleKernel,
            OutlineType.Gaussian => gaussianKernel,
            OutlineType.PixelPerfect => pixelPerfectKernel,
            _ => simpleKernel
        };

        VecI offset = new VecI(finalKernel.RadiusX, finalKernel.RadiusY);
        double gain = 1.0 / finalKernel.Sum;

        filter?.Dispose();
        filter = ImageFilter.CreateMatrixConvolution(finalKernel, (float)gain, 0, offset, TileMode.Clamp, true);

        lastType = Type.Value;
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (Background.Value == null)
        {
            return;
        }

        if (Thickness.Value > 0)
        {
            paint.ImageFilter = filter;
            paint.ColorFilter = ColorFilter.CreateBlendMode(Color.Value, BlendMode.SrcIn);

            using Texture temp = Texture.ForProcessing(surface, context.ProcessingColorSpace);
            int saved = temp.DrawingSurface.Canvas.SaveLayer(paint);

            var ctx = context.Clone();
            ctx.ChunkResolution = ChunkResolution.Full;
            bool isAdjusted = context.DocumentSize == context.RenderOutputSize;
            ctx.RenderOutputSize = isAdjusted ? context.RenderOutputSize : (VecI)(context.RenderOutputSize * context.ChunkResolution.InvertedMultiplier());

            Background.Value.Paint(ctx, temp.DrawingSurface);

            temp.DrawingSurface.Canvas.RestoreToCount(saved);

            temp.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);

            int thickness = (int)(Thickness.Value * context.ChunkResolution.Multiplier());

            for (int i = 1; i < (int)thickness; i++)
            {
                saved = temp.DrawingSurface.Canvas.SaveLayer(paint);

                temp.DrawingSurface.Canvas.DrawSurface(temp.DrawingSurface, 0, 0);

                temp.DrawingSurface.Canvas.RestoreToCount(saved);
            }

            saved = surface.Canvas.Save();
            surface.Canvas.SetMatrix(Matrix3X3.Identity);
            surface.Canvas.DrawSurface(temp.DrawingSurface, 0, 0);

            surface.Canvas.RestoreToCount(saved);
        }

        Background?.Value?.Paint(context, surface);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return new RectD(0, 0, lastDocumentSize.X, lastDocumentSize.Y);
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        int saved = renderOn.Canvas.Save();
        renderOn.Canvas.Scale((float)context.ChunkResolution.Multiplier());
        OnPaint(context, renderOn);
        renderOn.Canvas.RestoreToCount(saved);
        return true;
    }

    public override Node CreateCopy()
    {
        return new OutlineNode();
    }
}

public enum OutlineType
{
    Simple,
    Gaussian,
    PixelPerfect,
}
