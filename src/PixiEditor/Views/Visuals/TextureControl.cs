using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
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
        Texture texture = Texture;
        texture.GpuSurface.Flush();
        ICustomDrawOperation drawOperation = new DrawTextureOperation(
            new Rect(0, 0, Bounds.Width, Bounds.Height),
            Stretch,
            texture);

        context.Custom(drawOperation);
    }
}

internal class DrawTextureOperation : SkiaDrawOperation
{
    public Stretch Stretch { get; }
    public VecD TargetSize { get; }
    public Texture Texture { get; }

    public DrawTextureOperation(Rect dirtyBounds, Stretch stretch, Texture texture) :
        base(dirtyBounds)
    {
        Stretch = stretch;
        Texture = texture;
        TargetSize = new VecD(texture.Size.X, texture.Size.Y);
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        SKCanvas canvas = lease.SkCanvas;
        
        Texture.GpuSurface.Canvas.Flush();
        
        (DrawingBackendApi.Current.SurfaceImplementation as SkiaSurfaceImplementation).GrContext = lease.GrContext;
        
        canvas.Save();
        ScaleCanvas(canvas);
        canvas.DrawSurface(Texture.GpuSurface.Native as SKSurface, 0, 0); 
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
