using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;
using PixiEditor.Views.Visuals;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Rendering;

public class UniversalScene : Zoombox.Zoombox, ICustomHitTest
{
    public static readonly StyledProperty<SceneRenderer> SceneRendererProperty = AvaloniaProperty.Register<UniversalScene, SceneRenderer>(
        nameof(SceneRenderer));

    public SceneRenderer SceneRenderer
    {
        get => GetValue(SceneRendererProperty);
        set => SetValue(SceneRendererProperty, value);
    }
    
    public override void Render(DrawingContext context)
    {
        // TODO: Do bounds pass, that will be used to calculate dirty bounds
        
        if (SceneRenderer is null)
        {
            return;
        }
        
        using var drawOperation = new DrawUniversalSceneOperation(SceneRenderer.RenderScene, Bounds, CalculateTransformMatrix());
        context.Custom(drawOperation);

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }

    bool ICustomHitTest.HitTest(Point point)
    {
        return Bounds.Contains(point);
    }

    private Matrix CalculateTransformMatrix()
    {
        Matrix transform = Matrix.Identity;
        transform = transform.Append(Matrix.CreateRotation((float)AngleRadians));
        transform = transform.Append(Matrix.CreateScale(FlipX ? -1 : 1, FlipY ? -1 : 1));
        transform = transform.Append(Matrix.CreateScale((float)Scale, (float)Scale));
        transform = transform.Append(Matrix.CreateTranslation(CanvasPos.X, CanvasPos.Y));
        return transform;
    }
}

class DrawUniversalSceneOperation : SkiaDrawOperation
{
    public Action<DrawingSurface> RenderScene;
    public Matrix TransformMatrix { get; set; }

    public DrawUniversalSceneOperation(Action<DrawingSurface> renderAction, Rect dirtyBounds,
        Matrix calculateTransformMatrix) : base(dirtyBounds)
    {
        RenderScene = renderAction;
        TransformMatrix = calculateTransformMatrix;
    }

    public override bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        var originalMatrix = lease.SkSurface.Canvas.TotalMatrix;

        lease.SkSurface.Canvas.SetMatrix(TransformMatrix.ToSKMatrix());
        DrawingSurface surface = DrawingSurface.FromNative(lease.SkSurface);
        RenderScene?.Invoke(surface);

        DrawDebugGrid(lease.SkSurface.Canvas);
        lease.SkSurface.Canvas.SetMatrix(originalMatrix);
    }

    private void DrawDebugGrid(SKCanvas canvas)
    {
        canvas.DrawText("(0, 0)", 5, -5, new SKPaint() { Color = SKColors.White });
        canvas.DrawCircle(0, 0, 5, new SKPaint() { Color = SKColors.White });
        
        canvas.DrawText("(100, 100)", 105, 95, new SKPaint() { Color = SKColors.White });
        canvas.DrawCircle(100, 100, 5, new SKPaint() { Color = SKColors.White });
        
        canvas.DrawText("(-100, -100)", -105, -95, new SKPaint() { Color = SKColors.White });
        canvas.DrawCircle(-100, -100, 5, new SKPaint() { Color = SKColors.White });
        
        canvas.DrawText("(100, -100)", 105, -95, new SKPaint() { Color = SKColors.White });
        canvas.DrawCircle(100, -100, 5, new SKPaint() { Color = SKColors.White });
        
        canvas.DrawText("(-100, 100)", -105, 95, new SKPaint() { Color = SKColors.White });
        canvas.DrawCircle(-100, 100, 5, new SKPaint() { Color = SKColors.White });
        
        for (int i = -1000; i < 1000; i += 100)
        {
            canvas.DrawLine(i, -1000, i, 1000, new SKPaint() { Color = SKColors.White });
            canvas.DrawLine(-1000, i, 1000, i, new SKPaint() { Color = SKColors.White });
        }
    }
}
