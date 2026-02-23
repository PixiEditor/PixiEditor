using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;
using DrawingApiBlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Rendering;

public class RenderContext
{
    public int CloneDepth { get; protected init; } = 0;
    public double Opacity { get; set; }

    public KeyFrameTime FrameTime { get; set; }
    public ChunkResolution ChunkResolution { get; set; }
    public RectI? VisibleDocumentRegion { get; set; } = null;
    public SamplingOptions DesiredSamplingOptions { get; set; } = SamplingOptions.Default;
    public VecI RenderOutputSize { get; set; }

    public VecI DocumentSize { get; set; }
    public Canvas RenderSurface { get; set; }
    public bool FullRerender { get; set; } = false;
    public PointerInfo PointerInfo { get; set; }
    public KeyboardInfo KeyboardInfo { get; set; }
    public EditorData EditorData { get; set; }
    public ViewportData ViewportData { get; set; }
    public ColorSpace ProcessingColorSpace { get; set; }
    public string? TargetOutput { get; set; }
    public AffectedArea AffectedArea { get; set; }
    public Dictionary<Guid, List<PreviewRenderRequest>>? PreviewTextures { get; set; }
    public IReadOnlyNodeGraph Graph { get; set; }

    public static RenderContext Empty { get; } = new RenderContext(
        null,
        new KeyFrameTime(),
        ChunkResolution.Full,
        new VecI(1, 1),
        new VecI(1, 1),
        ColorSpace.CreateSrgb(),
        SamplingOptions.Default,
        new NodeGraph());


    public RenderContext(Canvas renderSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution,
        VecI renderOutputSize, VecI documentSize, ColorSpace processingColorSpace, SamplingOptions desiredSampling, IReadOnlyNodeGraph graph, double opacity = 1)
    {
        RenderSurface = renderSurface;
        FrameTime = frameTime;
        ChunkResolution = chunkResolution;
        RenderOutputSize = renderOutputSize;
        Opacity = opacity;
        ProcessingColorSpace = processingColorSpace;
        DocumentSize = documentSize;
        DesiredSamplingOptions = desiredSampling;
        Graph = graph;
    }

    public List<PreviewRenderRequest>? GetPreviewTexturesForNode(Guid id)
    {
        if (PreviewTextures is null)
            return null;
        PreviewTextures.TryGetValue(id, out List<PreviewRenderRequest> requests);
        PreviewTextures.Remove(id);
        return requests;
    }

    public static DrawingApiBlendMode GetDrawingBlendMode(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Normal => DrawingApiBlendMode.SrcOver,
            BlendMode.Erase => DrawingApiBlendMode.DstOut,
            BlendMode.Darken => DrawingApiBlendMode.Darken,
            BlendMode.Multiply => DrawingApiBlendMode.Multiply,
            BlendMode.ColorBurn => DrawingApiBlendMode.ColorBurn,
            BlendMode.Lighten => DrawingApiBlendMode.Lighten,
            BlendMode.Screen => DrawingApiBlendMode.Screen,
            BlendMode.ColorDodge => DrawingApiBlendMode.ColorDodge,
            BlendMode.LinearDodge => DrawingApiBlendMode.Plus,
            BlendMode.Overlay => DrawingApiBlendMode.Overlay,
            BlendMode.SoftLight => DrawingApiBlendMode.SoftLight,
            BlendMode.HardLight => DrawingApiBlendMode.HardLight,
            BlendMode.Difference => DrawingApiBlendMode.Difference,
            BlendMode.Exclusion => DrawingApiBlendMode.Exclusion,
            BlendMode.Hue => DrawingApiBlendMode.Hue,
            BlendMode.Saturation => DrawingApiBlendMode.Saturation,
            BlendMode.Luminosity => DrawingApiBlendMode.Luminosity,
            BlendMode.Color => DrawingApiBlendMode.Color,
            _ => DrawingApiBlendMode.SrcOver,
        };
    }

    public virtual RenderContext Clone()
    {
        return new RenderContext(RenderSurface, FrameTime, ChunkResolution, RenderOutputSize, DocumentSize, ProcessingColorSpace, DesiredSamplingOptions, Graph, Opacity)
        {
            FullRerender = FullRerender,
            TargetOutput = TargetOutput,
            AffectedArea = AffectedArea,
            PreviewTextures = PreviewTextures,
            VisibleDocumentRegion = VisibleDocumentRegion,
            PointerInfo = PointerInfo,
            EditorData = EditorData,
            KeyboardInfo = KeyboardInfo,
            ViewportData = ViewportData,
            CloneDepth = CloneDepth + 1
        };
    }
}
