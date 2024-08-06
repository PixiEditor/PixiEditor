using System;
using PixiEditor.DrawingApi.Core.Bridge;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Extensions;

public static class DrawingBackendExtensions
{
    private static RenderGraphicsContext? _renderGraphicsContext;
    public static IDisposable RenderOnDifferentGrContext(this IDrawingBackend drawingBackend, GRContext targetContext)
    {
        if (drawingBackend is not SkiaDrawingBackend skiaDrawingBackend)
        {
            throw new InvalidOperationException("This extension method can only be used with SkiaDrawingBackend.");
        }

        if (_renderGraphicsContext == null)
        {
            _renderGraphicsContext = new RenderGraphicsContext(skiaDrawingBackend.GraphicsContext, skiaDrawingBackend.SurfaceImplementation);
        }
        
        _renderGraphicsContext.Target = targetContext;
        
        return _renderGraphicsContext;
    }
}
