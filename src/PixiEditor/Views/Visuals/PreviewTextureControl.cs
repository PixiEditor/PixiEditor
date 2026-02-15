using Avalonia;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Interop.Avalonia.Core.Controls;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Visuals;

public class PreviewTextureControl : DrawieControl
{
    public static readonly StyledProperty<TexturePreview?> TexturePreviewProperty =
        AvaloniaProperty.Register<PreviewTextureControl, TexturePreview?>(
            nameof(TexturePreview));

    public static readonly StyledProperty<TileMode> TileModeXProperty =
        AvaloniaProperty.Register<PreviewTextureControl, TileMode>(
            nameof(TileModeX), TileMode.Decal);

    public static readonly StyledProperty<TileMode> TileModeYProperty = AvaloniaProperty.Register<PreviewTextureControl, TileMode>(
        nameof(TileModeY), TileMode.Decal);

    public TileMode TileModeY
    {
        get => GetValue(TileModeYProperty);
        set => SetValue(TileModeYProperty, value);
    }

    public static readonly StyledProperty<double> SourceWidthProperty =
        AvaloniaProperty.Register<PreviewTextureControl, double>(
            nameof(SourceWidth), double.NaN);

    public static readonly StyledProperty<double> SourceHeightProperty =
        AvaloniaProperty.Register<PreviewTextureControl, double>(
            nameof(SourceHeight), double.NaN);

    public double SourceHeight
    {
        get => GetValue(SourceHeightProperty);
        set => SetValue(SourceHeightProperty, value);
    }

    public double SourceWidth
    {
        get => GetValue(SourceWidthProperty);
        set => SetValue(SourceWidthProperty, value);
    }

    public TileMode TileModeX
    {
        get => GetValue(TileModeXProperty);
        set => SetValue(TileModeXProperty, value);
    }

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
            TexturePreview.Attach(this, GetBounds);
            TexturePreview.TextureUpdated += QueueNextFrame;
        }
    }

    private VecI GetBounds()
    {
        double width = double.IsPositive(Width) ? Width : Bounds.Width;
        width = double.IsPositive(SourceWidth) ? SourceWidth : width;

        double height = double.IsPositive(Height) ? Height : Bounds.Height;

        height = double.IsPositive(SourceHeight) ? SourceHeight : height;

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
        if (TexturePreview is { Preview: not null } && TexturePreview.Preview is { IsDisposed: false })
        {
            VecD scaling = new(Bounds.Size.Width / TexturePreview.Preview.Size.X,
                Bounds.Size.Height / TexturePreview.Preview.Size.Y);
            surface.Canvas.Save();
            surface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            if (TileModeX != TileMode.Decal || TileModeY != TileMode.Decal)
            {
                using var snapshot = TexturePreview.Preview.DrawingSurface.Snapshot();
                var localMatrix = Matrix3X3.CreateScale(
                    TexturePreview.Preview.Size.X / (float)Bounds.Size.Width,
                    TexturePreview.Preview.Size.Y / (float)Bounds.Size.Height
                );
                using var shader = Shader.CreateImage(snapshot, TileModeX, TileModeY, localMatrix);
                using Paint paint = new() { Shader = shader };

                surface.Canvas.DrawRect(0, 0, TexturePreview.Preview.Size.X, TexturePreview.Preview.Size.Y, paint);
            }
            else
            {
                surface.Canvas.DrawSurface(TexturePreview.Preview.DrawingSurface, 0, 0);
            }

            surface.Canvas.Restore();
        }
    }

    private static void OnTexturePreviewChanged(AvaloniaPropertyChangedEventArgs<TexturePreview?> args)
    {
        if (args.Sender is PreviewTextureControl control)
        {
            args.OldValue.Value?.Detach(control);
            if (args.OldValue.Value != null)
                args.OldValue.Value.TextureUpdated -= control.QueueNextFrame;
            args.NewValue.Value?.Attach(control, () => control.GetBounds());
            if (args.NewValue.Value != null)
                args.NewValue.Value.TextureUpdated += control.QueueNextFrame;
        }
    }
}
