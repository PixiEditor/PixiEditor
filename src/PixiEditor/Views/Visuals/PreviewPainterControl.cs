using Avalonia;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Interop.Avalonia.Core.Controls;
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

    static PreviewPainterControl()
    {
        PreviewPainterProperty.Changed.Subscribe(PainterChanged);
        BoundsProperty.Changed.Subscribe(UpdatePainterBounds);
    }

    public PreviewPainterControl()
    {
        
    }

    public PreviewPainterControl(PreviewPainter previewPainter, int frameToRender)
    {
        PreviewPainter = previewPainter;
        FrameToRender = frameToRender;
    }

    private static void PainterChanged(AvaloniaPropertyChangedEventArgs<PreviewPainter> args)
    {
        var sender = args.Sender as PreviewPainterControl;
        if (args.OldValue.Value != null)
        {
            args.OldValue.Value.RequestMatrix -= sender.OnPainterRequestMatrix;
            args.OldValue.Value.RequestRepaint -= sender.OnPainterRenderRequest;
        }

        if (args.NewValue.Value != null)
        {
            args.NewValue.Value.RequestMatrix += sender.OnPainterRequestMatrix;
            args.NewValue.Value.RequestRepaint += sender.OnPainterRenderRequest;
            
            args.NewValue.Value.Repaint();
        }
    }

    private void OnPainterRenderRequest()
    {
        QueueNextFrame();
    }

    public override void Draw(DrawingSurface surface)
    {
        if (PreviewPainter == null)
        {
            return;
        }

        PreviewPainter.Paint(surface);
    }

    private Matrix3X3 UniformScale(float x, float y, RectD previewBounds)
    {
        float scaleX = (float)Bounds.Width / x;
        float scaleY = (float)Bounds.Height / y;
        var scale = Math.Min(scaleX, scaleY);
        float dX = (float)Bounds.Width / 2 / scale - x / 2;
        dX -= (float)previewBounds.X;
        float dY = (float)Bounds.Height / 2 / scale - y / 2;
        dY -= (float)previewBounds.Y;
        Matrix3X3 matrix = Matrix3X3.CreateScale(scale, scale);
        return matrix.Concat(Matrix3X3.CreateTranslation(dX, dY));
    }

    private static void UpdatePainterBounds(AvaloniaPropertyChangedEventArgs<Rect> args)
    {
        var sender = args.Sender as PreviewPainterControl;
        if(sender == null) return;
        
        if (sender.PreviewPainter == null)
        {
            return;
        }

        sender.PreviewPainter.Bounds = new VecI((int)sender.Bounds.Width, (int)sender.Bounds.Height);
        sender.PreviewPainter.Repaint();
    }

    private Matrix3X3? OnPainterRequestMatrix()
    {
        RectD? previewBounds =
            PreviewPainter?.PreviewRenderable?.GetPreviewBounds(FrameToRender, PreviewPainter.ElementToRenderName);

        if (previewBounds == null || previewBounds.Value.IsZeroOrNegativeArea)
        {
            return null;
        }

        float x = (float)(previewBounds?.Width ?? 0);
        float y = (float)(previewBounds?.Height ?? 0);

        return UniformScale(x, y, previewBounds.Value);
    }
}
