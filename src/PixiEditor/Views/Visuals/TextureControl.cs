using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.DrawingApi.Skia.Extensions;
using PixiEditor.DrawingApi.Skia.Implementations;
using PixiEditor.Numerics;

namespace PixiEditor.Views.Visuals;

public class TextureControl : Control
{
    public static readonly StyledProperty<Texture> TextureProperty = AvaloniaProperty.Register<TextureControl, Texture>(
        nameof(Texture));

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<TextureControl, Stretch>(
        nameof(Stretch), Stretch.Uniform);

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public Texture Texture
    {
        get => GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }

    static TextureControl()
    {
        AffectsRender<TextureControl>(TextureProperty, StretchProperty);
    }

    public TextureControl()
    {
        ClipToBounds = true;
        TextureProperty.Changed.Subscribe(OnTextureChanged);
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = Texture;
        var result = new Size();

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, new Size(source.Size.X, source.Size.Y));
        }
        else if (Width > 0 && Height > 0)
        {
            result = Stretch.CalculateSize(availableSize, new Size(Width, Height));
        }

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Texture;

        if (source != null)
        {
            var sourceSize = source.Size;
            var result = Stretch.CalculateSize(finalSize, new Size(sourceSize.X, sourceSize.Y));
            return result;
        }
        else
        {
            return Stretch.CalculateSize(finalSize, new Size(Width, Height));
        }

        return new Size();
    }

    public override void Render(DrawingContext context)
    {
        if (Texture == null)
        {
            return;
        }
        
        Texture texture = Texture;
        texture.Surface.Flush();
        ICustomDrawOperation drawOperation = new DrawTextureOperation(
            new Rect(0, 0, Bounds.Width, Bounds.Height),
            Stretch,
            texture);

        context.Custom(drawOperation);
    }
    
    private void OnTextureChanged(AvaloniaPropertyChangedEventArgs<Texture> args)
    {
        if (args.OldValue.Value != null)
        {
            args.OldValue.Value.Changed -= TextureOnChanged;
        }
        
        if (args.NewValue.Value != null)
        {
            args.NewValue.Value.Changed += TextureOnChanged;
        }
    }
    
    private void TextureOnChanged(RectD? changedRect)
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }
}

internal class DrawTextureOperation : SkiaDrawOperation
{
    public Stretch Stretch { get; }
    public VecD TargetSize { get; }
    public Texture? Texture { get; }
    public Paint? Paint { get; }

    public DrawTextureOperation(Rect dirtyBounds, Stretch stretch, Texture texture, Paint paint = null) :
        base(dirtyBounds)
    {
        Stretch = stretch;
        Texture = texture;
        TargetSize = new VecD(texture.Size.X, texture.Size.Y);
        Paint = paint;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        if (Texture == null || Texture.IsDisposed)
        {
            return;
        }
        
        SKCanvas canvas = lease.SkCanvas;

        using var ctx = DrawingBackendApi.Current.RenderOnDifferentGrContext(lease.GrContext);

        canvas.Save();
        ScaleCanvas(canvas);
        canvas.DrawSurface(Texture.Surface.Native as SKSurface, 0, 0, Paint?.Native as SKPaint ?? null);
        canvas.Restore();
    }

    private void ScaleCanvas(SKCanvas canvas)
    {
        float x = (float)TargetSize.X;
        float y = (float)TargetSize.Y;

        if (Stretch == Stretch.Fill)
        {
            canvas.Scale((float)Bounds.Width / x, (float)Bounds.Height / y);
        }
        else if (Stretch == Stretch.Uniform)
        {
            float scaleX = (float)Bounds.Width / x;
            float scaleY = (float)Bounds.Height / y;
            var scale = Math.Min(scaleX, scaleY);
            float dX = (float)Bounds.Width / 2 / scale - x / 2;
            float dY = (float)Bounds.Height / 2 / scale - y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
        else if (Stretch == Stretch.UniformToFill)
        {
            float scaleX = (float)Bounds.Width / x;
            float scaleY = (float)Bounds.Height / y;
            var scale = Math.Max(scaleX, scaleY);
            float dX = (float)Bounds.Width / 2 / scale - x / 2;
            float dY = (float)Bounds.Height / 2 / scale - y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
    }

    public override bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }
}
