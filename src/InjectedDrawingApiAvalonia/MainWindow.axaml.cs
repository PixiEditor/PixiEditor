using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using SkiaSharp;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;

namespace InjectedDrawingApiAvalonia;

public partial class MainWindow : Window
{
    CommandBuffer CommandBuffer = new CommandBuffer();
    public MainWindow()
    {
        InitializeComponent();
        SurfaceControl.Draw += SurfaceControlOnDraw;

        Task.Run(() =>
        {
            CommandBuffer.DrawRect(10, 10, 100, 100, new Paint { Color = Colors.Red });
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        SurfaceControl.InvalidateVisual();
    }

    private void SurfaceControlOnDraw(SKCanvas draw)
    {
        CommandBuffer.Dispatch(draw);
    }
}
