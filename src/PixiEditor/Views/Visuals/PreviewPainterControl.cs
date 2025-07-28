using Avalonia;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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

    public static readonly StyledProperty<VecI> CustomRenderSizeProperty =
        AvaloniaProperty.Register<PreviewPainterControl, VecI>(
            nameof(CustomRenderSize));

    public VecI CustomRenderSize
    {
        get => GetValue(CustomRenderSizeProperty);
        set => SetValue(CustomRenderSizeProperty, value);
    }

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
        CustomRenderSizeProperty.Changed.Subscribe(UpdatePainterBounds);
    }

    public PreviewPainterControl()
    {
    }

    public PreviewPainterControl(PreviewPainter previewPainter, int frameToRender)
    {
        PreviewPainter = previewPainter;
        FrameToRender = frameToRender;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (PreviewPainter != null && painterInstance != null)
        {
            PreviewPainter.RemovePainterInstance(painterInstance.RequestId);
            painterInstance.RequestMatrix = null;
            painterInstance.RequestRepaint = null;
            painterInstance = null;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (PreviewPainter != null && painterInstance == null)
        {
            painterInstance = PreviewPainter.AttachPainterInstance();
            VecI finalSize = GetFinalSize();
            if (finalSize is { X: > 0, Y: > 0 })
            {
                PreviewPainter.ChangeRenderTextureSize(painterInstance.RequestId, finalSize);
            }

            painterInstance.RequestMatrix = OnPainterRequestMatrix;
            painterInstance.RequestRepaint = OnPainterRenderRequest;

            PreviewPainter.RepaintFor(painterInstance.RequestId);
        }
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
            VecI finalSize = sender.GetFinalSize();
            if (finalSize is { X: > 0, Y: > 0 })
            {
                sender.PreviewPainter.ChangeRenderTextureSize(sender.painterInstance.RequestId, finalSize);
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

        if (CustomRenderSize.ShortestAxis > 0)
        {
            surface.Canvas.Save();
            VecI finalSize = GetFinalSize();
            surface.Canvas.Scale(
                (float)Bounds.Width / finalSize.X,
                (float)Bounds.Height / finalSize.Y);
        }

        PreviewPainter.Paint(surface, painterInstance.RequestId);

        if (CustomRenderSize.ShortestAxis > 0)
        {
            surface.Canvas.Restore();
        }
    }

    private Matrix3X3 UniformScale(float x, float y, RectD previewBounds)
    {
        VecI finalSize = GetFinalSize();
        float scaleX = finalSize.X / x;
        float scaleY = finalSize.Y / y;
        var scale = Math.Min(scaleX, scaleY);
        float dX = (float)finalSize.X / 2 / scale - x / 2;
        dX -= (float)previewBounds.X;
        float dY = (float)finalSize.Y / 2 / scale - y / 2;
        dY -= (float)previewBounds.Y;
        Matrix3X3 matrix = Matrix3X3.CreateScale(scale, scale);
        return matrix.Concat(Matrix3X3.CreateTranslation(dX, dY));
    }

    private VecI GetFinalSize()
    {
        VecI finalSize = CustomRenderSize.ShortestAxis > 0
            ? CustomRenderSize
            : new VecI((int)Bounds.Width, (int)Bounds.Height);
        if (Bounds.Width < finalSize.X && Bounds.Height < finalSize.Y)
        {
            finalSize = new VecI((int)Bounds.Width, (int)Bounds.Height);
        }

        return finalSize;
    }

    private static void UpdatePainterBounds(AvaloniaPropertyChangedEventArgs args)
    {
        var sender = args.Sender as PreviewPainterControl;

        if (sender?.PreviewPainter == null)
        {
            return;
        }

        if (sender.painterInstance != null)
        {
            VecI finalSize = sender.GetFinalSize();
            if (finalSize is { X: > 0, Y: > 0 })
            {
                sender.PreviewPainter.ChangeRenderTextureSize(sender.painterInstance.RequestId, finalSize);
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
