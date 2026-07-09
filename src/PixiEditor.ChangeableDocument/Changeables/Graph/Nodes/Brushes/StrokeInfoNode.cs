using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo("StrokeInfo")]
public class StrokeInfoNode : Node, IBrushSampleTextureNode
{
    public OutputProperty<float> StrokeWidth { get; }
    public OutputProperty<VecI> ComputedSampleSize { get; }
    public OutputProperty<int> Stamp { get; }
    public OutputProperty<VecD> StartPoint { get; }
    public OutputProperty<VecD> LastAppliedPoint { get; }
    public OutputProperty<Texture> LatestSampleTexture { get; }
    public OutputProperty<Texture> StartingSampleTexture { get; }
    public OutputProperty<Texture> TargetSampleTexture { get; }
    public OutputProperty<VecD> TargetSampleTexturePos { get; }
    public OutputProperty<VecD> StartingSampleTexturePos { get; }
    public OutputProperty<Texture> TargetFullTexture { get; }
    public OutputProperty<Texture> LatestFullTexture { get; }
    public OutputProperty<Texture> StartingFullTexture { get; }
    public OutputProperty<Texture> SampleStateBuffer { get; }
    public OutputProperty<Texture> FullStateBuffer { get; }

    private TextureCache cache;

    public StrokeInfoNode()
    {
        StrokeWidth = CreateOutput<float>("StrokeWidth", "STROKE_WIDTH", 1f);
        Stamp = CreateOutput<int>("Stamp", "STAMP_NUMBER", 1);
        ComputedSampleSize = CreateOutput<VecI>("ComputedSampleSize", "COMPUTED_SAMPLE_SIZE", VecI.Zero);
        StartPoint = CreateOutput<VecD>("StartPoint", "START_POINT", VecD.Zero);
        LastAppliedPoint = CreateOutput<VecD>("LastAppliedPoint", "LAST_APPLIED_POINT", VecD.Zero);
        TargetSampleTexture = CreateOutput<Texture>("TargetSampleTexture", "TARGET_SAMPLE_TEXTURE", null);
        LatestSampleTexture = CreateOutput<Texture>("LatestSampleTexture", "LATEST_SAMPLE_TEXTURE", null);
        TargetSampleTexturePos = CreateOutput<VecD>("TargetSampleTexturePos", "TARGET_SAMPLE_TEXTURE_POS", VecD.Zero);
        StartingSampleTexture = CreateOutput<Texture>("StartingSampleTexture", "STARTING_SAMPLE_TEXTURE", null);
        StartingSampleTexturePos =
            CreateOutput<VecD>("StartingSampleTexturePos", "STARTING_SAMPLE_TEXTURE_POS", VecD.Zero);
        TargetFullTexture = CreateOutput<Texture>("TargetFullTexture", "TARGET_FULL_TEXTURE", null);
        LatestFullTexture = CreateOutput<Texture>("LatestFullTexture", "LATEST_FULL_TEXTURE", null);
        StartingFullTexture = CreateOutput<Texture>("StartingFullTexture", "STARTING_FULL_TEXTURE", null);

        SampleStateBuffer = CreateOutput<Texture>("SampleStateBuffer", "SAMPLE_STATE_BUFFER", null);
        FullStateBuffer = CreateOutput<Texture>("FullStateBuffer", "FULL_STATE_BUFFER", null);

        cache = new TextureCache();
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context is not BrushRenderContext brushRenderContext)
            return;

        StrokeWidth.Value = brushRenderContext.BrushData.StrokeWidth;
        Stamp.Value = brushRenderContext.Stamp;
        StartPoint.Value = brushRenderContext.StartPoint;
        LastAppliedPoint.Value = brushRenderContext.LastAppliedPoint;
        TargetSampleTexturePos.Value = brushRenderContext.LatestSampleTexturePos;
        StartingSampleTexturePos.Value = brushRenderContext.StartingSampleTexturePos;

        if (TargetSampleTexture.Connections.Count > 0)
        {
            TargetSampleTexture.Value = brushRenderContext.TargetSampleTexture;
        }

        if (StartingSampleTexture.Connections.Count > 0)
        {
            StartingSampleTexture.Value = brushRenderContext.StartingSampleTexture;
        }

        if (LatestSampleTexture.Connections.Count > 0)
        {
            LatestSampleTexture.Value = brushRenderContext.LatestSampledTexture;
        }

        if (TargetFullTexture.Connections.Count > 0)
        {
            TargetFullTexture.Value = brushRenderContext.LatestFullTexture ?? brushRenderContext.StartingFullTexture;
        }

        if (StartingFullTexture.Connections.Count > 0)
        {
            StartingFullTexture.Value = brushRenderContext.StartingFullTexture;
        }

        if (LatestFullTexture.Connections.Count > 0)
        {
            LatestFullTexture.Value = brushRenderContext.LatestFullTexture;
        }

        ComputedSampleSize.Value = brushRenderContext.TargetSampleTexture?.Size ?? VecI.Zero;

        if (SampleStateBuffer.Connections.Count > 0 && brushRenderContext.StateBuffer != null)
        {
            SampleStateBuffer.Value = UpdateTextureUnderRect("SampleStateBuffer", brushRenderContext.StateBuffer, new RectI((VecI)brushRenderContext.LatestSampleTexturePos, brushRenderContext.TargetSampleTexture.Size), brushRenderContext.ProcessingColorSpace);
        }

        if (FullStateBuffer.Connections.Count > 0)
        {
            FullStateBuffer.Value = UpdateTextureUnderRect("FullStateBuffer", brushRenderContext.StateBuffer, new RectI(VecI.Zero, brushRenderContext.StateBuffer.LatestSize), brushRenderContext.ProcessingColorSpace);
        }
    }

    private Texture UpdateTextureUnderRect(string cacheId, ChunkyImage target, RectI rect, ColorSpace colorSpace)
    {
        var surfaceUnderRect = cache.RequestTexture(cacheId, rect.Size, colorSpace);

        target.DrawMostUpToDateRegionOn(rect, ChunkResolution.Full, surfaceUnderRect.DrawingSurface.Canvas,
            VecI.Zero);

        return surfaceUnderRect;
    }

    public override Node CreateCopy()
    {
        return new StrokeInfoNode();
    }

    public override void Dispose()
    {
        cache.Dispose();
        base.Dispose();
    }
}
