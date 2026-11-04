using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("DisplacementMap")]
public class DisplacementMapNode : FilterNode
{
    public InputProperty<ColorChannel> XChannel { get; }
    public InputProperty<ColorChannel> YChannel { get; }
    public InputProperty<float> Scale { get; }
    public InputProperty<Texture> Displacement { get; }
    public InputProperty<ResamplingMethod> DisplacementSampling { get; }

    private Drawie.Backend.Core.Surfaces.ImageData.Image lastImg;
    private ImageFilter? lastDisplacementFilter;

    public DisplacementMapNode()
    {
        XChannel = CreateInput("XChannel", "X_CHANNEL", ColorChannel.R);
        YChannel = CreateInput("YChannel", "Y_CHANNEL", ColorChannel.G);
        Scale = CreateInput("Scale", "SCALE", 1f);
        Displacement = CreateInput<Texture>("Displacement", "DISPLACEMENT", null);
        DisplacementSampling = CreateInput<ResamplingMethod>("Sampling", "SAMPLING", ResamplingMethod.NearestNeighbor);
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        if (Displacement.Value == null) return null;

        lastDisplacementFilter?.Dispose();
        lastImg?.Dispose();
        lastImg = Displacement.Value.DrawingSurface.Snapshot();
        lastDisplacementFilter = ImageFilter.CreateImage(lastImg, ResamplingToSampling(DisplacementSampling.Value));

        return ImageFilter.CreateDisplacementMap(XChannel.Value, YChannel.Value, Scale.Value, lastDisplacementFilter);
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
