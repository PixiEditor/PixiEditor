using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;
using DrawingApiBlendMode = PixiEditor.DrawingApi.Core.Surface.BlendMode;

namespace PixiEditor.ChangeableDocument.Rendering;
public class RenderingContext : IDisposable
{
    public Paint BlendModePaint = new () { BlendMode = DrawingApiBlendMode.SrcOver };
    public Paint BlendModeOpacityPaint = new () { BlendMode = DrawingApiBlendMode.SrcOver };
    public Paint ReplacingPaintWithOpacity = new () { BlendMode = DrawingApiBlendMode.Src };

    public KeyFrameTime FrameTime { get; }
    public VecI? ChunkToUpdate { get; }
    public ChunkResolution? Resolution { get; }

    public RenderingContext(KeyFrameTime frameTime)
    {
        FrameTime = frameTime;
    }
    
    public RenderingContext(KeyFrameTime frameTime, VecI chunkToUpdate, ChunkResolution resolution)
    {
        FrameTime = frameTime;
        ChunkToUpdate = chunkToUpdate;
        Resolution = resolution;
    }

    public static DrawingApiBlendMode GetDrawingBlendMode(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Normal => DrawingApiBlendMode.SrcOver,
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

    public void Dispose()
    {
        BlendModePaint.Dispose();
        BlendModeOpacityPaint.Dispose();
        ReplacingPaintWithOpacity.Dispose();
    }
}
