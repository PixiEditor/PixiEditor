using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace DrawingApi.Benchmarks;

public class SurfaceBenchmarks
{
    public GraphicsContext GraphicsContext { get; set; }

    public DrawingSurface CreateDrawingSurface(VecI size)
    {
        ImageInfo info = new(size.X, size.Y);
        if (GraphicsContext == GraphicsContext.CPU)
        {
            return DrawingSurface.Create(info);
        }

        if (GraphicsContext == GraphicsContext.OpenGL)
        {
            return DrawingSurface.Create(info, true);
        }

        return null;
    }
    
    public DrawingSurface MergeTwoSurfaces()
    {
        DrawingSurface surface1 = CreateDrawingSurface(new VecI(10000, 10000));
        DrawingSurface surface2 = CreateDrawingSurface(new VecI(10000, 10000));
        surface2.Canvas.DrawRect(0, 0, 10000, 10000, new Paint() { Color = new Color(255, 0, 0) });

        surface1.Canvas.DrawSurface(surface2, 0, 0, new Paint());
        
        return surface1;
    }

    public Image Snapshot(DrawingSurface surface)
    {
        return surface.Snapshot();
    }
    
    public void PeekPixels(Image image)
    { 
        image.PeekPixels();
    }

    public void PickColor(Pixmap pixmap, int x, int y)
    {
        pixmap.GetColor(x, y);
    }
}
