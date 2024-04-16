using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace PixiEditor.AvaloniaUI.Views.Visuals;

public class CheckerBackground : Control
{
    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<CheckerBackground, double>(
        nameof(Scale));

    public static readonly StyledProperty<string> CheckerImagePathProperty = AvaloniaProperty.Register<Scene, string>(
        nameof(CheckerImagePath));

    public static readonly StyledProperty<int> PixelWidthProperty = AvaloniaProperty.Register<CheckerBackground, int>(
        nameof(PixelWidth));

    public static readonly StyledProperty<float> PixelHeightProperty = AvaloniaProperty.Register<CheckerBackground, float>(
        nameof(PixelHeight));

    public float PixelHeight
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

    private Brush? _checkerBrush;


    static CheckerBackground()
    {
        CheckerImagePathProperty.Changed.AddClassHandler<CheckerBackground>(CheckerImagePathChanged);
    }

    public CheckerBackground()
    {
    }

    private static void CheckerImagePathChanged(CheckerBackground control, AvaloniaPropertyChangedEventArgs arg2)
    {
        if (arg2.NewValue is string path)
        {
            control.CheckerBitmap = ImagePathToBitmapConverter.LoadBitmapFromRelativePath(path);
            control.CreateCheckerPen();
        }
        else
        {
            control.CheckerBitmap = null;
        }
    }

    private void CreateCheckerPen()
    {

    }


    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (CheckerBitmap != null)
        {
            float checkerScale = (float)ZoomToViewportConverter.ZoomToViewport(16, Scale);
            _checkerBrush = new ImageBrush(CheckerBitmap)
            {
                TileMode = TileMode.Tile,
                DestinationRect = new RelativeRect(0, 0, checkerScale, checkerScale, RelativeUnit.Absolute),
            };

            _checkerBrush.Transform = new ScaleTransform(0.5f, 0.5f);
        }
        context.PushRenderOptions(new RenderOptions() { BitmapInterpolationMode = BitmapInterpolationMode.None });
        if (_checkerBrush != null)
        {
            context.DrawRectangle(_checkerBrush, null, new Rect(new Size(PixelWidth, PixelHeight)));
        }
    }
}
