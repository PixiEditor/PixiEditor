using System;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;
using DrawingApiBlendMode = PixiEditor.DrawingApi.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Rendering;

public class RenderContext : IDisposable
{
    public Paint BlendModePaint = new() { BlendMode = DrawingApiBlendMode.SrcOver };
    public Paint BlendModeOpacityPaint = new() { BlendMode = DrawingApiBlendMode.SrcOver };
    public Paint ReplacingPaintWithOpacity = new() { BlendMode = DrawingApiBlendMode.Src };

    public KeyFrameTime FrameTime { get; }
    public ChunkResolution ChunkResolution { get; }
    public VecI DocumentSize { get; set; }
    
    public DrawingSurface TargetSurface { get; set; }

    public bool IsDisposed { get; private set; }
    
    public RenderContext(DrawingSurface targetSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution, VecI docSize)
    {
        TargetSurface = targetSurface;
        FrameTime = frameTime;
        ChunkResolution = chunkResolution;
        DocumentSize = docSize;
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

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        
        IsDisposed = true;
        BlendModePaint.Dispose();
        BlendModeOpacityPaint.Dispose();
        ReplacingPaintWithOpacity.Dispose();
    }
}
