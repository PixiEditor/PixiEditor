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
    public ChunkResolution ChunkResolution { get; }
    public VecI RenderOutputSize { get; set; }

    public VecI DocumentSize { get; set; }
    public DrawingSurface RenderSurface { get; set; }
    public bool FullRerender { get; set; } = false;
    
    public ColorSpace ProcessingColorSpace { get; set; }
    public string? TargetOutput { get; set; }   


    public RenderContext(DrawingSurface renderSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution,
        VecI renderOutputSize, VecI documentSize, ColorSpace processingColorSpace, double opacity = 1)
    {
        RenderSurface = renderSurface;
        FrameTime = frameTime;
        ChunkResolution = chunkResolution;
        RenderOutputSize = renderOutputSize;
        Opacity = opacity;
        ProcessingColorSpace = processingColorSpace;
        DocumentSize = documentSize;
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
}
