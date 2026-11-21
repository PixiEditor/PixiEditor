using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("DisplacementMap")]
public class DisplacementMapNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<ColorChannel> XChannel { get; }
    public InputProperty<ColorChannel> YChannel { get; }
    public InputProperty<float> Scale { get; }
    public InputProperty<Texture> Displacement { get; }
    public InputProperty<ResamplingMethod> DisplacementSampling { get; }

    private Drawie.Backend.Core.Surfaces.ImageData.Image lastImg;
    private ImageFilter? lastDisplacementImageFilter;
    private ImageFilter? lastDisplacementFilter;
    private Paint paint;


    public DisplacementMapNode()
    {
        Background = CreateRenderInput("Background", "BACKGROUND");
        XChannel = CreateInput("XChannel", "X_CHANNEL", ColorChannel.R);
        YChannel = CreateInput("YChannel", "Y_CHANNEL", ColorChannel.G);
        Scale = CreateInput("Scale", "SCALE", 1f);
        Displacement = CreateInput<Texture>("Displacement", "DISPLACEMENT_PROPERTY", null);
        DisplacementSampling = CreateInput<ResamplingMethod>("Sampling", "SAMPLING_PROPERTY", ResamplingMethod.NearestNeighbor);
        AllowHighDpiRendering = false;
        paint = new Paint();
        paint.BlendMode = BlendMode.Src;
        Output.FirstInChain = null;
    }

    protected override void OnPaint(RenderContext context, Canvas surface)
    {
        if (Background.Value == null) return;
        if (Displacement.Value == null)
        {
            Background.Value.Paint(context, surface);
            return;
        }

        using Texture temp = Texture.ForProcessing(surface, context.ProcessingColorSpace);
        Background.Value.Paint(context, temp.DrawingSurface.Canvas);

        using var snapshot = temp.DrawingSurface.Snapshot();
        using var imgFilter = ImageFilter.CreateImage(snapshot, ResamplingToSampling(DisplacementSampling.Value));

        lastDisplacementFilter?.Dispose();
        lastDisplacementFilter = GetImageFilter(context, imgFilter);

        paint.ImageFilter = lastDisplacementFilter;

        var saved = surface.Save();
        surface.SetMatrix(Matrix3X3.Identity);
        surface.DrawPaint(paint);
        surface.RestoreToCount(saved);
    }

    private ImageFilter? GetImageFilter(RenderContext context, ImageFilter backgroundImageFilter)
    {
        if (Displacement.Value == null) return null;

        lastDisplacementImageFilter?.Dispose();
        lastImg?.Dispose();

        lastImg = Displacement.Value.DrawingSurface.Snapshot();
        lastDisplacementImageFilter =
            ImageFilter.CreateImage(lastImg, ResamplingToSampling(DisplacementSampling.Value));

        float scaledScale = Scale.Value * (float)context.ChunkResolution.Multiplier();

        return ImageFilter.CreateDisplacementMap(XChannel.Value, YChannel.Value, scaledScale,
            lastDisplacementImageFilter, backgroundImageFilter);
    }

    private SamplingOptions ResamplingToSampling(ResamplingMethod samplingValue)
    {
        return samplingValue switch
        {
            ResamplingMethod.NearestNeighbor => SamplingOptions.Default,
            ResamplingMethod.Bilinear => SamplingOptions.Bilinear,
            ResamplingMethod.Bicubic => SamplingOptions.Bicubic,
            _ => throw new ArgumentOutOfRangeException(nameof(samplingValue), samplingValue, null)
        };
    }

    public override Node CreateCopy()
    {
        return new DisplacementMapNode();
    }
}
