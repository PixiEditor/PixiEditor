using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Skia;

namespace InjectedDrawingApiAvalonia;

public partial class MainWindow : Window
{
    public static readonly StyledProperty<DrawingSurface> SurfaceProperty = AvaloniaProperty.Register<MainWindow, DrawingSurface>(
        "Surface");

    public DrawingSurface Surface
    {
        get => GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }
    
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        Surface = DrawingSurface.Create(new ImageInfo(128, 128));
        
        Surface.Canvas.Clear(Colors.Brown);
        Surface.Canvas.DrawRect(0, 0, 128, 128, new Paint(){ Color = Colors.Green });
        Surface.Canvas.Flush();
        SurfaceControl.InvalidateVisual();
    }
}
