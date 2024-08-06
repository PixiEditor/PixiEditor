using System;
using PixiEditor.DrawingApi.Skia.Implementations;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia;

public class RenderGraphicsContext : IDisposable
{
    private GRContext? _target;
    public GRContext Original { get; }

    public GRContext Target
    {
        get => _target;
        set
        {
            if (_target != null)
            {
                throw new InvalidOperationException("Target is already set.");
            }
            
            _target = value;
            SurfaceImplementation.GrContext = value;
        }
    }
    public SkiaSurfaceImplementation SurfaceImplementation { get; }
    
    public RenderGraphicsContext(GRContext context, SkiaSurfaceImplementation surfaceImplementation)
    {
        Original = context;
        SurfaceImplementation = surfaceImplementation;
    }
    
    public void Dispose()
    {
        SurfaceImplementation.GrContext = Original;
        _target = null;
    }
}
