using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;
using DrawingApiBlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Rendering;

public class RenderContext
{
    public double Opacity { get; set; }

    public KeyFrameTime FrameTime { get; }
    public ChunkResolution ChunkResolution { get; set; }
    public RectI? VisibleDocumentRegion { get; set; } = null;
    public SamplingOptions DesiredSamplingOptions { get; set; } = SamplingOptions.Default;
    public VecI RenderOutputSize { get; set; }

    public VecI DocumentSize { get; set; }
    public DrawingSurface RenderSurface { get; set; }
    public bool FullRerender { get; set; } = false;
    
    public ColorSpace ProcessingColorSpace { get; set; }
    public string? TargetOutput { get; set; }
    public AffectedArea AffectedArea { get; set; }
    public Dictionary<Guid, Texture>? PreviewTextures { get; set; }


    public RenderContext(DrawingSurface renderSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution,
        VecI renderOutputSize, VecI documentSize, ColorSpace processingColorSpace, SamplingOptions desiredSampling, double opacity = 1)
    {
        RenderSurface = renderSurface;
        FrameTime = frameTime;
        ChunkResolution = chunkResolution;
        RenderOutputSize = renderOutputSize;
        Opacity = opacity;
        ProcessingColorSpace = processingColorSpace;
        DocumentSize = documentSize;
        DesiredSamplingOptions = desiredSampling;
    }

    public Texture? GetPreviewTexture(Guid guid)
    {
        if (PreviewTextures is null)
            return null;
        PreviewTextures.TryGetValue(guid, out Texture? texture);
        PreviewTextures.Remove(guid);
        return texture;
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

    public RenderContext Clone()
    {
        return new RenderContext(RenderSurface, FrameTime, ChunkResolution, RenderOutputSize, DocumentSize, ProcessingColorSpace, DesiredSamplingOptions, Opacity)
        {
            FullRerender = FullRerender,
            TargetOutput = TargetOutput,
            AffectedArea = AffectedArea,
            PreviewTextures = PreviewTextures,
            VisibleDocumentRegion = VisibleDocumentRegion
        };
    }
}
