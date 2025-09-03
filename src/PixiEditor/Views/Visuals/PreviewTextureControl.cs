using Avalonia;
using Drawie.Backend.Core.Surfaces;
using Drawie.Interop.Avalonia.Core.Controls;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Visuals;

public class PreviewTextureControl : DrawieControl
{
    public static readonly StyledProperty<TexturePreview?> TexturePreviewProperty =
        AvaloniaProperty.Register<PreviewTextureControl, TexturePreview?>(
            nameof(TexturePreview));

    public TexturePreview? TexturePreview
    {
        get => GetValue(TexturePreviewProperty);
        set => SetValue(TexturePreviewProperty, value);
    }

    static PreviewTextureControl()
    {
        AffectsRender<PreviewTextureControl>(TexturePreviewProperty);
        TexturePreviewProperty.Changed.Subscribe(OnTexturePreviewChanged);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TexturePreview != null)
        {
            TexturePreview.Attach(this, () =>
            {
                return GetBounds();
            });
            TexturePreview.TextureUpdated += QueueNextFrame;
        }
    }

    private VecI GetBounds()
    {
        double width = double.IsPositive(Width) ? Width : Bounds.Width;
        double height = double.IsPositive(Height) ? Height : Bounds.Height;
        if (double.IsNaN(width) || double.IsInfinity(width))
            width = Bounds.Width;
        if (double.IsNaN(height) || double.IsInfinity(height))
            height = Bounds.Height;

        return new VecI((int)Math.Ceiling(width), (int)Math.Ceiling(height));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        TexturePreview?.Detach(this);
        if (TexturePreview != null)
            TexturePreview.TextureUpdated -= QueueNextFrame;
    }

    public override void Draw(DrawingSurface surface)
    {
        if (TexturePreview is { Preview: not null })
        {
            VecD scaling = new(Bounds.Size.Width / TexturePreview.Preview.Size.X, Bounds.Size.Height / TexturePreview.Preview.Size.Y);
            surface.Canvas.Save();
            surface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            surface.Canvas.DrawSurface(TexturePreview.Preview.DrawingSurface, 0, 0);
            surface.Canvas.Restore();
        }
    }

    private static void OnTexturePreviewChanged(AvaloniaPropertyChangedEventArgs<TexturePreview?> args)
    {
        if (args.Sender is PreviewTextureControl control)
        {
            args.OldValue.Value?.Detach(control);
            if(args.OldValue.Value != null)
                args.OldValue.Value.TextureUpdated -= control.QueueNextFrame;
            args.NewValue.Value?.Attach(control, () => control.GetBounds());
            if(args.NewValue.Value != null)
                args.NewValue.Value.TextureUpdated += control.QueueNextFrame;
        }
    }
}
