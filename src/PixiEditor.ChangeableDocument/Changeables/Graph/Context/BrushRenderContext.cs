using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public class BrushRenderContext : RenderContext
{
    public BrushData BrushData { get; }
    public Texture? LatestSampledTexture { get; set; }
    public Texture? LatestFullTexture { get; }
    public Texture? TargetSampleTexture { get; set; }
    public Texture? StartingSampleTexture { get; }
    public Texture? StartingFullTexture { get; }
    public VecD StartPoint { get; }
    public VecD LastAppliedPoint { get; }
    public VecD LatestSampleTexturePos { get; }
    public VecD StartingSampleTexturePos { get; }
    public bool DryRun { get; set; } = false;
    public int Stamp { get; set; }

    public BrushRenderContext(Canvas? renderSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution, VecI renderOutputSize, VecI documentSize, ColorSpace processingColorSpace, SamplingOptions desiredSampling, BrushData brushData, Texture? targetSampleTexture, Texture? latestSampledTexture, VecD latestSampleTexturePos, Texture? startingSampleTexture, VecD startingSampleTexturePos, Texture? targetStartingFullTexture, Texture? latestFullTexture, IReadOnlyNodeGraph graph, VecD startPoint, VecD lastAppliedPoint, int stamp, int graphCacheId, double opacity = 1) : base(renderSurface, frameTime, chunkResolution, renderOutputSize, documentSize, processingColorSpace, desiredSampling, graph, opacity)
    {
        BrushData = brushData;
        StartPoint = startPoint;
        LastAppliedPoint = lastAppliedPoint;
        Stamp = stamp;
        TargetSampleTexture = targetSampleTexture;
        LatestSampledTexture = latestSampledTexture;
        LatestSampleTexturePos = latestSampleTexturePos;
        LatestFullTexture = latestFullTexture;
        StartingFullTexture = targetStartingFullTexture;
        StartingSampleTexture = startingSampleTexture;
        StartingSampleTexturePos = startingSampleTexturePos;
        GraphCacheId = graphCacheId;
    }

    public override RenderContext Clone()
    {
        return new BrushRenderContext(RenderSurface, FrameTime, ChunkResolution, RenderOutputSize, DocumentSize,
            ProcessingColorSpace, DesiredSamplingOptions, BrushData, TargetSampleTexture, LatestSampledTexture, LatestSampleTexturePos, StartingSampleTexture, StartingSampleTexturePos, StartingFullTexture, LatestFullTexture, Graph,
            StartPoint, LastAppliedPoint, Stamp, GraphCacheId, Opacity)
        {
            VisibleDocumentRegion = VisibleDocumentRegion,
            AffectedArea = AffectedArea,
            FullRerender = FullRerender,
            TargetOutput = TargetOutput,
            PreviewTextures = PreviewTextures,
            EditorData = EditorData,
            KeyboardInfo = KeyboardInfo,
            PointerInfo = PointerInfo,
            ViewportData = ViewportData,
            CloneDepth = CloneDepth + 1,
        };
    }
}
