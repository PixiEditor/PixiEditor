using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.Rendering;
using Drawie.Numerics;

namespace PixiEditor.Views.Visuals;

public class PreviewPainterControl : Control
{
    public static readonly StyledProperty<int> FrameToRenderProperty =
        AvaloniaProperty.Register<PreviewPainterControl, int>("FrameToRender");

    public static readonly StyledProperty<PreviewPainter> PreviewPainterProperty =
        AvaloniaProperty.Register<PreviewPainterControl, PreviewPainter>(
            nameof(PreviewPainter));

    public PreviewPainter PreviewPainter
    {
        get => GetValue(PreviewPainterProperty);
        set => SetValue(PreviewPainterProperty, value);
    }

    public int FrameToRender
    {
        get { return (int)GetValue(FrameToRenderProperty); }
        set { SetValue(FrameToRenderProperty, value); }
    }

    public PreviewPainterControl()
    {
        PreviewPainterProperty.Changed.Subscribe(PainterChanged);
    }

    public override void Render(DrawingContext context)
    {
        if (PreviewPainter == null)
        {
            return;
        }

        using var renderOperation = new DrawPreviewOperation(Bounds, PreviewPainter, FrameToRender);
        context.Custom(renderOperation);
    }

    private void PainterChanged(AvaloniaPropertyChangedEventArgs<PreviewPainter> args)
    {
        if (args.OldValue.Value != null)
        {
            args.OldValue.Value.RequestRepaint -= OnPainterRenderRequest;
        }

        if (args.NewValue.Value != null)
        {
            args.NewValue.Value.RequestRepaint += OnPainterRenderRequest;
        }
    }

    private void OnPainterRenderRequest()
    {
        InvalidateVisual();
    }
}

internal class DrawPreviewOperation : SkiaDrawOperation
{
    public PreviewPainter PreviewPainter { get; }
    private int frame;

    public DrawPreviewOperation(Rect dirtyBounds, PreviewPainter previewPainter, int frameToRender) : base(dirtyBounds)
    {
        PreviewPainter = previewPainter;
        frame = frameToRender;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        RectD? previewBounds = PreviewPainter.PreviewRenderable.GetPreviewBounds(frame, PreviewPainter.ElementToRenderName);
        if (PreviewPainter == null)
        {
            return;
        }

        DrawingSurface target = DrawingSurface.FromNative(lease.SkSurface);

        float x = (float)(previewBounds?.Width ?? 0); 
        float y = (float)(previewBounds?.Height ?? 0);

        target.Canvas.Save();

        if (previewBounds != null)
        {
            UniformScale(x, y, target, previewBounds.Value);
        }

        // TODO: Implement ChunkResolution and frame
        PreviewPainter.Paint(target, ChunkResolution.Full, frame);

        target.Canvas.Restore();

        DrawingSurface.Unmanage(target);
    }

    private void UniformScale(float x, float y, DrawingSurface target, RectD previewBounds)
    {
        float scaleX = (float)Bounds.Width / x;
        float scaleY = (float)Bounds.Height / y;
        var scale = Math.Min(scaleX, scaleY);
        float dX = (float)Bounds.Width / 2 / scale - x / 2;
        dX -= (float)previewBounds.X; 
        float dY = (float)Bounds.Height / 2 / scale - y / 2;
        dY -= (float)previewBounds.Y;
        target.Canvas.Scale(scale, scale);
        target.Canvas.Translate(dX, dY);
    }

    public override bool Equals(ICustomDrawOperation? other)
    {
        return other is DrawPreviewOperation operation && operation.PreviewPainter == PreviewPainter;
    }
}
