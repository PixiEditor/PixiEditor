using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
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
    public List<ISceneObject> SceneObjects { get; set; } = new List<ISceneObject>();

    public override void Render(DrawingContext context)
    {
        // TODO: Do bounds pass, that will be used to calculate dirty bounds
        var drawOperation = new DrawUniversalSceneOperation(SceneObjects, Bounds, CalculateTransformMatrix());
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
    public List<ISceneObject> SceneObjects { get; set; }
    public Matrix TransformMatrix { get; set; }

    public DrawUniversalSceneOperation(List<ISceneObject> sceneObjects, Rect dirtyBounds,
        Matrix calculateTransformMatrix) : base(dirtyBounds)
    {
        SceneObjects = sceneObjects;
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

        foreach (ISceneObject sceneObject in SceneObjects)
        {
            RenderObject(lease, sceneObject);
        }

        DrawDebugGrid(lease.SkSurface.Canvas);
        lease.SkSurface.Canvas.SetMatrix(originalMatrix);
    }

    private static void RenderObject(ISkiaSharpApiLease lease, ISceneObject sceneObject)
    {
        DrawingSurface surface = DrawingSurface.FromNative(lease.SkSurface);
        sceneObject.RenderInScene(surface);
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

public class RenderContext
{
    public DrawingSurface Surface { get; }
    public RectD LocalBounds { get; }
    
    public RenderContext(DrawingSurface surface, RectD localBounds)
    {
        Surface = surface;
        LocalBounds = localBounds;
    }
}

public class RenderGraph
{
    public List<EffectNode> Nodes { get; set; } = new List<EffectNode>();

    public void RenderInLocalSpace(RenderContext context)
    {
        foreach (EffectNode node in Nodes)
        {
            node.Render(context);
        }
    }
}

public abstract class EffectNode
{
    public abstract void Render(RenderContext context);
}

class DrawRectNode : EffectNode
{
    public override void Render(RenderContext context)
    {
        context.Surface.Canvas.DrawRect(0, 0, (int)context.LocalBounds.Width, (int)context.LocalBounds.Height, new Paint 
            { Color = Colors.Aquamarine, BlendMode = BlendMode.Difference} );
    }
}

class ApplyEffectNode : EffectNode
{
    public override void Render(RenderContext context)
    {
        using Paint paint = new Paint();
        paint.ColorFilter = Filters.RedGrayscaleFilter;

        context.Surface.Canvas.DrawPaint(paint);
    }
}
