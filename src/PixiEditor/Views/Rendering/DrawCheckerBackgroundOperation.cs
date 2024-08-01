using Avalonia;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using PixiEditor.Helpers.Converters;
using PixiEditor.Views.Visuals;

namespace PixiEditor.Views.Rendering;

internal class DrawCheckerBackgroundOperation : SkiaDrawOperation
{
    public SKBitmap CheckerBitmap { get; set; }
    public SKRect SurfaceRectToRender { get; set; }

    private SKPaint _checkerPaint;

    public DrawCheckerBackgroundOperation(Rect dirtyBounds, SKBitmap checkerBitmap, float scale,
        SKRect surfaceRectToRender) : base(dirtyBounds)
    {
        SurfaceRectToRender = surfaceRectToRender;
        CheckerBitmap = checkerBitmap;

        float checkerScale = (float)ZoomToViewportConverter.ZoomToViewport(16, scale) * 0.25f;
        _checkerPaint = new SKPaint()
        {
            Shader = SKShader.CreateBitmap(
                CheckerBitmap,
                SKShaderTileMode.Repeat, SKShaderTileMode.Repeat,
                SKMatrix.CreateScale(checkerScale, checkerScale)),
            FilterQuality = SKFilterQuality.None,
        };
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        var canvas = lease.SkCanvas;
        canvas.DrawRect(SurfaceRectToRender, _checkerPaint);
    }

    public override bool Equals(ICustomDrawOperation? other)
    {
        if (other is DrawCheckerBackgroundOperation operation)
        {
            return operation.CheckerBitmap == CheckerBitmap && operation.SurfaceRectToRender == SurfaceRectToRender;
        }

        return false;
    }
}
