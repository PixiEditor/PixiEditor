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

    private PainterInstance? painterInstance;

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
            if (sender.painterInstance != null)
            {
                args.OldValue.Value.RemovePainterInstance(sender.painterInstance.RequestId);
            }


            sender.painterInstance = null;
        }

        if (args.NewValue.Value != null)
        {
            sender.painterInstance = args.NewValue.Value.AttachPainterInstance();
            if (sender.Bounds is { Width: > 0, Height: > 0 })
            {
                sender.PreviewPainter.ChangeRenderTextureSize(sender.painterInstance.RequestId,
                    new VecI((int)sender.Bounds.Width, (int)sender.Bounds.Height));
            }

            sender.painterInstance.RequestMatrix = sender.OnPainterRequestMatrix;
            sender.painterInstance.RequestRepaint = sender.OnPainterRenderRequest;

            args.NewValue.Value.RepaintFor(sender.painterInstance.RequestId);
        }
        else
        {
            sender.painterInstance = null;
        }
    }

    private void OnPainterRenderRequest()
    {
        QueueNextFrame();
    }

    public override void Draw(DrawingSurface surface)
    {
        if (PreviewPainter == null || painterInstance == null)
        {
            return;
        }

        PreviewPainter.Paint(surface, painterInstance.RequestId);
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

        if (sender?.PreviewPainter == null)
        {
            return;
        }

        if (sender.painterInstance != null)
        {
            if (args.NewValue.Value is { Width: > 0, Height: > 0 })
            {
                sender.PreviewPainter.ChangeRenderTextureSize(sender.painterInstance.RequestId,
                    new VecI((int)args.NewValue.Value.Width, (int)args.NewValue.Value.Height));
                sender.PreviewPainter.RepaintFor(sender.painterInstance.RequestId);
            }
        }
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
