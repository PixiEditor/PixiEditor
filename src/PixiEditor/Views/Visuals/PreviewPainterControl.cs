using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Interop.VulkanAvalonia.Controls;
using PixiEditor.Models.Rendering;
using Drawie.Numerics;

namespace PixiEditor.Views.Visuals;

public class PreviewPainterControl : DrawieControl
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
        QueueNextFrame();
    }

    public override void Draw(DrawingSurface surface)
    {
        RectD? previewBounds =
            PreviewPainter.PreviewRenderable.GetPreviewBounds(FrameToRender, PreviewPainter.ElementToRenderName);
        if (PreviewPainter == null)
        {
            return;
        }

        float x = (float)(previewBounds?.Width ?? 0);
        float y = (float)(previewBounds?.Height ?? 0);

        surface.Canvas.Save();

        if (previewBounds != null)
        {
            UniformScale(x, y, surface, previewBounds.Value);
        }

        // TODO: Implement ChunkResolution and frame
        PreviewPainter.Paint(surface, ChunkResolution.Full, FrameToRender);

        surface.Canvas.Restore();
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
}
