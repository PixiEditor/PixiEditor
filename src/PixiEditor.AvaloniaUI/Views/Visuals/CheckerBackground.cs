using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

public class CheckerBackground : Control
{
    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<CheckerBackground, double>(
        nameof(Scale));

    public static readonly StyledProperty<string> CheckerImagePathProperty = AvaloniaProperty.Register<Scene, string>(
        nameof(CheckerImagePath));

    public static readonly StyledProperty<int> PixelWidthProperty = AvaloniaProperty.Register<CheckerBackground, int>(
        nameof(PixelWidth));

    public static readonly StyledProperty<int> PixelHeightProperty = AvaloniaProperty.Register<CheckerBackground, int>(
        nameof(PixelHeight));

    public int PixelHeight
    {
        get => GetValue(PixelHeightProperty);
        set => SetValue(PixelHeightProperty, value);
    }

    public int PixelWidth
    {
        get => GetValue(PixelWidthProperty);
        set => SetValue(PixelWidthProperty, value);
    }

    public string CheckerImagePath
    {
        get => GetValue(CheckerImagePathProperty);
        set => SetValue(CheckerImagePathProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public Bitmap? CheckerBitmap { get; set; }


    static CheckerBackground()
    {
        CheckerImagePathProperty.Changed.AddClassHandler<CheckerBackground>(CheckerImagePathChanged);
    }

    private static void CheckerImagePathChanged(CheckerBackground control, AvaloniaPropertyChangedEventArgs arg2)
    {
        if (arg2.NewValue is string path)
        {
            control.CheckerBitmap = ImagePathToBitmapConverter.LoadDrawingApiBitmapFromRelativePath(path);
        }
        else
        {
            control.CheckerBitmap = null;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DrawCheckerboardOperation drawCheckerboardOperation = new DrawCheckerboardOperation(new Rect(0, 0, PixelWidth, PixelHeight), new VecI(PixelWidth, PixelHeight), (SKBitmap)CheckerBitmap.Native, (float)Scale);
        context.Custom(drawCheckerboardOperation);
    }
}

internal class DrawCheckerboardOperation : SkiaDrawOperation
{
    private SKPaint paint;
    private SKBitmap checkerboardBitmap;
    private VecI pixelSize;

    public DrawCheckerboardOperation(Rect bounds, VecI pixelSize, SKBitmap bitmap, float scale) : base(bounds)
    {
        this.pixelSize = pixelSize;
        checkerboardBitmap = bitmap;
        float checkerScale = (float)ZoomToViewportConverter.ZoomToViewport(16, scale) * 0.25f;
        paint = new SKPaint()
        {
            Shader = SKShader.CreateBitmap(
                checkerboardBitmap,
                SKShaderTileMode.Repeat, SKShaderTileMode.Repeat,
                SKMatrix.CreateScale(checkerScale, checkerScale)),
            FilterQuality = SKFilterQuality.None
        };
    }

    public override bool Equals(ICustomDrawOperation? other)
    {
        return other is DrawCheckerboardOperation operation && operation.pixelSize == pixelSize;
    }

    public override void Render(ISkiaSharpApiLease lease)
    {
        lease.SkCanvas.DrawRect(Bounds.ToSKRect(), paint);
    }
}
