using Drawie.Backend.Core;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo("StrokeInfo")]
public class StrokeInfoNode : Node, IBrushSampleTextureNode
{
    public OutputProperty<float> StrokeWidth { get; }
    public OutputProperty<VecI> ComputedSampleSize { get; }
    public OutputProperty<VecD> StartPoint { get; }
    public OutputProperty<VecD> LastAppliedPoint { get; }
    public OutputProperty<Texture> TargetSampleTexture { get; }
    public OutputProperty<VecD> TargetSampleTexturePos { get; }
    public OutputProperty<Texture> TargetFullTexture { get; }
    public OutputProperty<Texture> AdditionalSampleTexture { get; }
    public OutputProperty<Texture> AdditionalFullTexture { get; }

    private TextureCache cache = new TextureCache();

    public StrokeInfoNode()
    {
        StrokeWidth = CreateOutput<float>("StrokeWidth", "STROKE_WIDTH", 1f);
        ComputedSampleSize = CreateOutput<VecI>("ComputedSampleSize", "COMPUTED_SAMPLE_SIZE", VecI.Zero);
        StartPoint = CreateOutput<VecD>("StartPoint", "START_POINT", VecD.Zero);
        LastAppliedPoint = CreateOutput<VecD>("LastAppliedPoint", "LAST_APPLIED_POINT", VecD.Zero);
        TargetSampleTexture = CreateOutput<Texture>("TargetSampleTexture", "TARGET_SAMPLE_TEXTURE", null);
        TargetSampleTexturePos = CreateOutput<VecD>("TargetSampleTexturePos", "TARGET_SAMPLE_TEXTURE_POS", VecD.Zero);
        TargetFullTexture = CreateOutput<Texture>("TargetFullTexture", "TARGET_FULL_TEXTURE", null);
        AdditionalSampleTexture = CreateOutput<Texture>("AdditionalSampleTexture", "ADDITIONAL_SAMPLE_TEXTURE", null);
        AdditionalFullTexture = CreateOutput<Texture>("AdditionalFullTexture", "ADDITIONAL_FULL_TEXTURE", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context is not BrushRenderContext brushRenderContext)
            return;

        StrokeWidth.Value = brushRenderContext.BrushData.StrokeWidth;
        StartPoint.Value = brushRenderContext.StartPoint;
        LastAppliedPoint.Value = brushRenderContext.LastAppliedPoint;
        TargetSampleTexturePos.Value = brushRenderContext.TargetSampleTexturePos;

        if (TargetSampleTexture.Connections.Count > 0)
        {
            TargetSampleTexture.Value = brushRenderContext.TargetSampledTexture;
            ComputedSampleSize.Value = brushRenderContext.TargetSampledTexture?.Size ?? VecI.Zero;
        }

        if (TargetFullTexture.Connections.Count > 0)
        {
            TargetFullTexture.Value = brushRenderContext.TargetFullTexture;
        }

        if (AdditionalFullTexture.Connections.Count > 0 && brushRenderContext.Target?.Additional?.Count > 0)
        {
            Texture tex = cache.RequestTexture(brushRenderContext.GraphCacheId, brushRenderContext.DocumentSize,
                brushRenderContext.Target.Main.ProcessingColorSpace);
            brushRenderContext.Target.Additional[0].DrawMostUpToDateRegionOn(
                new RectI(VecI.Zero, brushRenderContext.DocumentSize),
                ChunkResolution.Full,
                tex.DrawingSurface.Canvas,
                VecI.Zero);
            AdditionalFullTexture.Value = tex;
        }

        if (AdditionalSampleTexture.Connections.Count > 0 && brushRenderContext.Target?.Additional?.Count > 0)
        {
            Texture tex = cache.RequestTexture(brushRenderContext.GraphCacheId + 1, brushRenderContext.TargetSampledTexture?.Size ?? VecI.Zero,
                brushRenderContext.Target.Main.ProcessingColorSpace);
            brushRenderContext.Target.Additional[0].DrawMostUpToDateRegionOn(
                new RectI((VecI)brushRenderContext.TargetSampleTexturePos, brushRenderContext.TargetSampledTexture?.Size ?? VecI.Zero),
                ChunkResolution.Full,
                tex.DrawingSurface.Canvas,
                VecI.Zero);
            AdditionalSampleTexture.Value = tex;
        }
    }

    public override Node CreateCopy()
    {
        return new StrokeInfoNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        cache.Dispose();
    }
}
